using ABAPpartment.Application.DTOs.Incidents;
using ABAPpartment.Application.Interfaces;
using ABAPpartment.Domain.Entities;
using ABAPpartment.Domain.Interfaces;

namespace ABAPpartment.Application.Services;

public class IncidentService : IIncidentService
{
    private readonly IIncidentRepository _incidents;
    private readonly IApartmentRepository _apartments;
    private readonly IReservationRepository _reservations;
    private readonly IUserRepository _users;

    public IncidentService(
        IIncidentRepository incidents,
        IApartmentRepository apartments,
        IReservationRepository reservations,
        IUserRepository users)
    {
        _incidents = incidents;
        _apartments = apartments;
        _reservations = reservations;
        _users = users;
    }

    public async Task<IncidentDto> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var incident = await _incidents.GetByIdWithDetailsAsync(id, ct)
            ?? throw new KeyNotFoundException($"Incidencia {id} no encontrada.");
        return ToDto(incident);
    }

    public async Task<IEnumerable<IncidentSummaryDto>> GetAllAsync(CancellationToken ct = default)
        => (await _incidents.GetAllAsync(ct)).Select(ToSummary);

    public async Task<IEnumerable<IncidentSummaryDto>> GetByApartmentAsync(int apartmentId, CancellationToken ct = default)
        => (await _incidents.GetByApartmentAsync(apartmentId, ct)).Select(ToSummary);

    public async Task<IEnumerable<IncidentSummaryDto>> GetByOperatorAsync(int operatorId, CancellationToken ct = default)
        => (await _incidents.GetByAssignedOperatorAsync(operatorId, ct)).Select(ToSummary);

    public async Task<IEnumerable<IncidentSummaryDto>> GetByStatusAsync(string status, CancellationToken ct = default)
    {
        ValidateStatus(status);
        return (await _incidents.GetByStatusAsync(status, ct)).Select(ToSummary);
    }

    public async Task<IEnumerable<IncidentSummaryDto>> GetByCategoryAsync(string category, CancellationToken ct = default)
    {
        ValidateCategory(category);
        return (await _incidents.GetByCategoryAsync(category, ct)).Select(ToSummary);
    }

    public async Task<IEnumerable<IncidentSummaryDto>> GetByPriorityAsync(string priority, CancellationToken ct = default)
    {
        ValidatePriority(priority);
        return (await _incidents.GetByPriorityAsync(priority, ct)).Select(ToSummary);
    }

    public async Task<IncidentDashboardDto> GetDashboardAsync(CancellationToken ct = default)
    {
        var active = (await _incidents.GetActiveAsync(ct)).ToList();

        var open = active.Where(i => i.Status == IncidentStatus.Open).ToList();
        var inProgress = active.Where(i => i.Status == IncidentStatus.InProgress).ToList();

        var allResolved = (await _incidents.GetByStatusAsync(IncidentStatus.Resolved, ct)).ToList();

        var criticalAndHigh = active
            .Where(i => i.Priority is IncidentPriority.Critical or IncidentPriority.High)
            .OrderBy(i => i.Priority == IncidentPriority.Critical ? 0 : 1)
            .ThenBy(i => i.CreatedAt)
            .Take(10)
            .Select(ToSummary);

        var recentlyOpened = open
            .OrderByDescending(i => i.CreatedAt)
            .Take(5)
            .Select(ToSummary);

        return new IncidentDashboardDto(
            TotalOpen: open.Count,
            TotalInProgress: inProgress.Count,
            TotalResolved: allResolved.Count,
            CriticalOpen: open.Count(i => i.Priority == IncidentPriority.Critical),
            HighOpen: open.Count(i => i.Priority == IncidentPriority.High),
            UnassignedOpen: open.Count(i => i.AssignedToId == null),
            CriticalAndHigh: criticalAndHigh,
            RecentlyOpened: recentlyOpened
        );
    }

    public async Task<IncidentDto> CreateAsync(
        int reportedById,
        CreateIncidentRequest req,
        CancellationToken ct = default)
    {
        ValidateCategory(req.Category);
        ValidatePriority(req.Priority);

        if (string.IsNullOrWhiteSpace(req.Title))
            throw new ArgumentException("El título de la incidencia es obligatorio.");

        if (!await _apartments.ExistsAsync(req.ApartmentId, ct))
            throw new KeyNotFoundException($"Apartamento {req.ApartmentId} no encontrado.");

        if (req.ReservationId.HasValue)
        {
            var reservation = await _reservations.GetByIdAsync(req.ReservationId.Value, ct)
                ?? throw new KeyNotFoundException($"Reserva {req.ReservationId} no encontrada.");

            if (reservation.ApartmentId != req.ApartmentId)
                throw new InvalidOperationException(
                    "La reserva no pertenece al apartamento indicado.");
        }

        var assignedToId = await AutoAssignAsync(req.Category, ct);

        var incident = new Incident
        {
            ApartmentId = req.ApartmentId,
            ReservationId = req.ReservationId,
            ReportedById = reportedById,
            AssignedToId = assignedToId,
            Category = req.Category,
            Priority = req.Priority,
            Title = req.Title.Trim(),
            Description = req.Description?.Trim(),
            Status = IncidentStatus.Open,
            CreatedAt = DateTime.UtcNow,
        };

        await _incidents.AddAsync(incident, ct);
        await _incidents.SaveChangesAsync(ct);

        return await GetByIdAsync(incident.Id, ct);
    }

    public async Task<IncidentDto> UpdateAsync(
        int id,
        UpdateIncidentRequest req,
        CancellationToken ct = default)
    {
        var incident = await _incidents.GetByIdWithDetailsAsync(id, ct)
            ?? throw new KeyNotFoundException($"Incidencia {id} no encontrada.");

        if (incident.Status is IncidentStatus.Closed)
            throw new InvalidOperationException(
                "No se puede modificar una incidencia cerrada.");

        if (req.Title is not null) incident.Title = req.Title.Trim();
        if (req.Description is not null) incident.Description = req.Description.Trim();

        if (req.Priority is not null)
        {
            ValidatePriority(req.Priority);
            incident.Priority = req.Priority;
        }

        if (req.Category is not null)
        {
            ValidateCategory(req.Category);
            incident.Category = req.Category;
        }

        _incidents.Update(incident);
        await _incidents.SaveChangesAsync(ct);

        return ToDto(incident);
    }

    public async Task<IncidentDto> AssignAsync(
        int id,
        AssignIncidentRequest req,
        CancellationToken ct = default)
    {
        var incident = await _incidents.GetByIdWithDetailsAsync(id, ct)
            ?? throw new KeyNotFoundException($"Incidencia {id} no encontrada.");

        if (incident.Status is IncidentStatus.Resolved or IncidentStatus.Closed)
            throw new InvalidOperationException(
                "No se puede reasignar una incidencia resuelta o cerrada.");

        var operator_ = await _users.GetByIdAsync(req.AssignedToId, ct)
            ?? throw new KeyNotFoundException($"Usuario {req.AssignedToId} no encontrado.");

        if (operator_.Role is not (UserRole.Operator or UserRole.Admin))
            throw new InvalidOperationException(
                "Solo se puede asignar una incidencia a un Operator o Admin.");

        incident.AssignedToId = req.AssignedToId;

        if (incident.Status == IncidentStatus.Open)
            incident.Status = IncidentStatus.InProgress;

        _incidents.Update(incident);
        await _incidents.SaveChangesAsync(ct);

        return ToDto(incident);
    }
    public async Task<IncidentDto> UpdateStatusAsync(
        int id,
        UpdateIncidentStatusRequest req,
        CancellationToken ct = default)
    {
        ValidateStatus(req.Status);

        var incident = await _incidents.GetByIdWithDetailsAsync(id, ct)
            ?? throw new KeyNotFoundException($"Incidencia {id} no encontrada.");

        if (!IsTransitionAllowed(incident.Status, req.Status))
            throw new InvalidOperationException(
                $"No se puede cambiar el estado de '{incident.Status}' a '{req.Status}'.");

        incident.Status = req.Status;

        if (req.Status == IncidentStatus.Resolved)
        {
            incident.ResolvedAt = DateTime.UtcNow;
            if (req.ResolutionNote is not null)
                incident.Description += $"\n\n[Resolución]: {req.ResolutionNote}";
        }

        _incidents.Update(incident);
        await _incidents.SaveChangesAsync(ct);

        return ToDto(incident);
    }

    public async Task<IncidentDto> SyncZendeskAsync(
        int id,
        SyncZendeskRequest req,
        CancellationToken ct = default)
    {
        var incident = await _incidents.GetByIdWithDetailsAsync(id, ct)
            ?? throw new KeyNotFoundException($"Incidencia {id} no encontrada.");

        incident.ZendeskTicketId = req.ZendeskTicketId.Trim();
        _incidents.Update(incident);
        await _incidents.SaveChangesAsync(ct);

        return ToDto(incident);
    }

    public async Task CancelAsync(int id, CancellationToken ct = default)
    {
        var incident = await _incidents.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Incidencia {id} no encontrada.");

        if (incident.Status == IncidentStatus.Closed)
            throw new InvalidOperationException("La incidencia ya está cerrada.");

        incident.Status = IncidentStatus.Closed;
        _incidents.Update(incident);
        await _incidents.SaveChangesAsync(ct);
    }

    private async Task<int?> AutoAssignAsync(string category, CancellationToken ct)
    {
        var operators = await _users.GetAllAsync(ct);
        var available = operators
            .Where(u => u.Role == UserRole.Operator && u.IsActive)
            .FirstOrDefault();

        return available?.Id;
    }

    private static bool IsTransitionAllowed(string current, string next) =>
        (current, next) switch
        {
            (IncidentStatus.Open, IncidentStatus.InProgress) => true,
            (IncidentStatus.Open, IncidentStatus.Closed) => true,
            (IncidentStatus.InProgress, IncidentStatus.Resolved) => true,
            (IncidentStatus.InProgress, IncidentStatus.Closed) => true,
            (IncidentStatus.Resolved, IncidentStatus.Closed) => true,
            _ => false
        };
    private static void ValidateStatus(string status)
    {
        if (!IncidentStatus.All.Contains(status))
            throw new ArgumentException($"Estado inválido: {status}.");
    }
    private static void ValidateCategory(string category)
    {
        if (!IncidentCategory.All.Contains(category))
            throw new ArgumentException($"Categoría inválida: {category}.");
    }

    private static void ValidatePriority(string priority)
    {
        if (!IncidentPriority.All.Contains(priority))
            throw new ArgumentException($"Prioridad inválida: {priority}.");
    }

    private static IncidentDto ToDto(Incident i) => new(
        i.Id,
        i.ApartmentId,
        i.Apartment.Name,
        i.Apartment.District,
        i.ReservationId,
        i.ReportedById,
        i.ReportedBy.FullName,
        i.AssignedToId,
        i.AssignedTo?.FullName,
        i.Category,
        i.Priority,
        i.Title,
        i.Description,
        i.Status,
        i.ZendeskTicketId,
        i.ResolvedAt,
        i.CreatedAt
    );
    private static IncidentSummaryDto ToSummary(Incident i) => new(
        i.Id,
        i.Apartment.Name,
        i.Category,
        i.Priority,
        i.Title,
        i.AssignedTo?.FullName,
        i.Status,
        i.ZendeskTicketId,
        i.CreatedAt
    );
}