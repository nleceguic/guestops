using ABAPpartment.Application.DTOs.Cleaning;
using ABAPpartment.Application.Interfaces;
using ABAPpartment.Domain.Entities;
using ABAPpartment.Domain.Interfaces;

namespace ABAPpartment.Application.Services;

public class CleaningScheduleService : ICleaningScheduleService
{
    private readonly ICleaningScheduleRepository _cleanings;
    private readonly IApartmentRepository _apartments;
    private readonly IReservationRepository _reservations;
    private readonly IUserRepository _users;
    private static readonly TimeOnly DefaultCheckoutTime = new(11, 0);

    public CleaningScheduleService(
        ICleaningScheduleRepository cleanings,
        IApartmentRepository apartments,
        IReservationRepository reservations,
        IUserRepository users)
    {
        _cleanings = cleanings;
        _apartments = apartments;
        _reservations = reservations;
        _users = users;
    }

    public async Task<CleaningScheduleDto> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var c = await _cleanings.GetByIdWithDetailsAsync(id, ct)
            ?? throw new KeyNotFoundException($"Limpieza {id} no encontrada.");
        return ToDto(c);
    }

    public async Task<IEnumerable<CleaningScheduleSummaryDto>> GetAllAsync(CancellationToken ct = default)
        => (await _cleanings.GetAllAsync(ct)).Select(ToSummary);

    public async Task<IEnumerable<CleaningScheduleSummaryDto>> GetByApartmentAsync(int apartmentId, CancellationToken ct = default)
        => (await _cleanings.GetByApartmentAsync(apartmentId, ct)).Select(ToSummary);

    public async Task<IEnumerable<CleaningScheduleSummaryDto>> GetByOperatorAsync(int operatorId, CancellationToken ct = default)
        => (await _cleanings.GetByAssignedOperatorAsync(operatorId, ct)).Select(ToSummary);

    public async Task<IEnumerable<CleaningScheduleSummaryDto>> GetByStatusAsync(string status, CancellationToken ct = default)
    {
        ValidateStatus(status);
        return (await _cleanings.GetByStatusAsync(status, ct)).Select(ToSummary);
    }

    public async Task<DailyPlanningDto> GetDailyPlanningAsync(DateOnly date, CancellationToken ct = default)
    {
        var cleanings = (await _cleanings.GetByDateAsync(date, ct)).ToList();

        return new DailyPlanningDto(
            Date: date,
            Total: cleanings.Count,
            Scheduled: cleanings.Count(c => c.Status == CleaningStatus.Scheduled),
            InProgress: cleanings.Count(c => c.Status == CleaningStatus.InProgress),
            Done: cleanings.Count(c => c.Status == CleaningStatus.Done),
            Skipped: cleanings.Count(c => c.Status == CleaningStatus.Skipped),
            Cleanings: cleanings.Select(ToSummary)
        );
    }

    public async Task<CleaningScheduleDto> GenerateCheckoutCleaningAsync(
        int reservationId,
        CancellationToken ct = default)
    {
        var reservation = await _reservations.GetByIdWithDetailsAsync(reservationId, ct)
            ?? throw new KeyNotFoundException($"Reserva {reservationId} no encontrada.");

        var schedule = new CleaningSchedule
        {
            ApartmentId = reservation.ApartmentId,
            ReservationId = reservationId,
            ScheduledDate = reservation.CheckOutDate,
            ScheduledTime = DefaultCheckoutTime,
            Type = CleaningType.Checkout,
            Status = CleaningStatus.Scheduled,
        };

        await _cleanings.AddAsync(schedule, ct);
        await _cleanings.SaveChangesAsync(ct);

        return await GetByIdAsync(schedule.Id, ct);
    }

    public async Task<CleaningScheduleDto> CreateAsync(
        CreateCleaningScheduleRequest req,
        CancellationToken ct = default)
    {
        ValidateType(req.Type);

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

        if (req.AssignedToId.HasValue)
            await ValidateOperatorAsync(req.AssignedToId.Value, ct);

        if (req.ScheduledDate < DateOnly.FromDateTime(DateTime.UtcNow))
            throw new ArgumentException("No se puede programar una limpieza en el pasado.");

        var schedule = new CleaningSchedule
        {
            ApartmentId = req.ApartmentId,
            ReservationId = req.ReservationId,
            AssignedToId = req.AssignedToId,
            ScheduledDate = req.ScheduledDate,
            ScheduledTime = req.ScheduledTime,
            Type = req.Type,
            Status = CleaningStatus.Scheduled,
            Notes = req.Notes,
        };

        await _cleanings.AddAsync(schedule, ct);
        await _cleanings.SaveChangesAsync(ct);

        return await GetByIdAsync(schedule.Id, ct);
    }

    public async Task<CleaningScheduleDto> UpdateAsync(
        int id,
        UpdateCleaningScheduleRequest req,
        CancellationToken ct = default)
    {
        var schedule = await _cleanings.GetByIdWithDetailsAsync(id, ct)
            ?? throw new KeyNotFoundException($"Limpieza {id} no encontrada.");

        if (schedule.Status is CleaningStatus.Done or CleaningStatus.Skipped)
            throw new InvalidOperationException(
                "No se puede modificar una limpieza ya completada o descartada.");

        if (req.ScheduledDate.HasValue)
        {
            if (req.ScheduledDate.Value < DateOnly.FromDateTime(DateTime.UtcNow))
                throw new ArgumentException("No se puede reprogramar en el pasado.");
            schedule.ScheduledDate = req.ScheduledDate.Value;
        }

        if (req.ScheduledTime.HasValue) schedule.ScheduledTime = req.ScheduledTime.Value;
        if (req.Notes is not null) schedule.Notes = req.Notes;

        if (req.AssignedToId.HasValue)
        {
            await ValidateOperatorAsync(req.AssignedToId.Value, ct);
            schedule.AssignedToId = req.AssignedToId.Value;
        }

        _cleanings.Update(schedule);
        await _cleanings.SaveChangesAsync(ct);

        return ToDto(schedule);
    }

    public async Task<CleaningScheduleDto> UpdateStatusAsync(
        int id,
        UpdateCleaningStatusRequest req,
        CancellationToken ct = default)
    {
        ValidateStatus(req.Status);

        var schedule = await _cleanings.GetByIdWithDetailsAsync(id, ct)
            ?? throw new KeyNotFoundException($"Limpieza {id} no encontrada.");

        if (!IsTransitionAllowed(schedule.Status, req.Status))
            throw new InvalidOperationException(
                $"No se puede cambiar el estado de '{schedule.Status}' a '{req.Status}'.");

        schedule.Status = req.Status;

        if (req.Notes is not null)
            schedule.Notes = req.Notes;

        if (req.Status == CleaningStatus.Done)
            schedule.CompletedAt = DateTime.UtcNow;

        _cleanings.Update(schedule);
        await _cleanings.SaveChangesAsync(ct);

        return ToDto(schedule);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var schedule = await _cleanings.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Limpieza {id} no encontrada.");

        if (schedule.Status is CleaningStatus.InProgress or CleaningStatus.Done)
            throw new InvalidOperationException(
                "No se puede eliminar una limpieza en curso o completada.");

        schedule.Status = CleaningStatus.Skipped;
        _cleanings.Update(schedule);
        await _cleanings.SaveChangesAsync(ct);
    }

    private async Task ValidateOperatorAsync(int userId, CancellationToken ct)
    {
        var user = await _users.GetByIdAsync(userId, ct)
            ?? throw new KeyNotFoundException($"Usuario {userId} no encontrado.");

        if (user.Role is not (UserRole.Operator or UserRole.Admin))
            throw new InvalidOperationException(
                "Solo se puede asignar una limpieza a un Operator o Admin.");
    }

    private static bool IsTransitionAllowed(string current, string next) =>
        (current, next) switch
        {
            (CleaningStatus.Scheduled, CleaningStatus.InProgress) => true,
            (CleaningStatus.Scheduled, CleaningStatus.Skipped) => true,
            (CleaningStatus.InProgress, CleaningStatus.Done) => true,
            (CleaningStatus.InProgress, CleaningStatus.Skipped) => true,
            _ => false
        };

    private static void ValidateType(string type)
    {
        if (type is not (CleaningType.Checkout or CleaningType.Midstay
                      or CleaningType.Maintenance or CleaningType.Deep))
            throw new ArgumentException($"Tipo de limpieza inválido: {type}.");
    }

    private static void ValidateStatus(string status)
    {
        if (status is not (CleaningStatus.Scheduled or CleaningStatus.InProgress
                        or CleaningStatus.Done or CleaningStatus.Skipped))
            throw new ArgumentException($"Estado inválido: {status}.");
    }

    private static CleaningScheduleDto ToDto(CleaningSchedule c) => new(
        c.Id,
        c.ApartmentId,
        c.Apartment.Name,
        c.Apartment.District,
        c.ReservationId,
        c.AssignedToId,
        c.AssignedTo?.FullName,
        c.ScheduledDate,
        c.ScheduledTime,
        c.Type,
        c.Status,
        c.CompletedAt,
        c.Notes
    );

    private static CleaningScheduleSummaryDto ToSummary(CleaningSchedule c) => new(
        c.Id,
        c.Apartment.Name,
        c.Apartment.District,
        c.AssignedTo?.FullName,
        c.ScheduledDate,
        c.ScheduledTime,
        c.Type,
        c.Status
    );
}

public static class CleaningStatus
{
    public const string Scheduled = "Scheduled";
    public const string InProgress = "InProgress";
    public const string Done = "Done";
    public const string Skipped = "Skipped";
}

public static class CleaningType
{
    public const string Checkout = "Checkout";
    public const string Midstay = "Midstay";
    public const string Maintenance = "Maintenance";
    public const string Deep = "Deep";
}