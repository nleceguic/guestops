using ABAPpartment.Application.DTOs.Apartments;
using ABAPpartment.Application.Interfaces;
using ABAPpartment.Domain.Entities;
using ABAPpartment.Domain.Interfaces;

namespace ABAPpartment.Application.Services;

public class ApartmentService : IApartmentService
{
    private readonly IApartmentRepository _apartments;
    private readonly IUserRepository _users;
    private readonly IReservationRepository _reservations;
    private readonly IIncidentRepository _incidents;

    public ApartmentService(
        IApartmentRepository apartments,
        IUserRepository users,
        IReservationRepository reservations,
        IIncidentRepository incidents)
    {
        _apartments = apartments;
        _users = users;
        _reservations = reservations;
        _incidents = incidents;
    }

    public async Task<ApartmentDto> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var apt = await _apartments.GetByIdWithOwnerAsync(id, ct)
            ?? throw new KeyNotFoundException($"Apartamento {id} no encontrado.");

        return ToDto(apt);
    }

    public async Task<IEnumerable<ApartmentSummaryDto>> GetAllAsync(CancellationToken ct = default)
    {
        var list = await _apartments.GetAllAsync(ct);
        return list.Select(ToSummary);
    }

    public async Task<IEnumerable<ApartmentSummaryDto>> GetByOwnerAsync(int ownerId, CancellationToken ct = default)
    {
        var list = await _apartments.GetByOwnerAsync(ownerId, ct);
        return list.Select(ToSummary);
    }

    public async Task<IEnumerable<ApartmentSummaryDto>> GetByStatusAsync(string status, CancellationToken ct = default)
    {
        if (!IsValidStatus(status))
            throw new ArgumentException($"Estado inválido: {status}.");

        var list = await _apartments.GetByStatusAsync(status, ct);
        return list.Select(ToSummary);
    }

    public async Task<ApartmentMetricsDto> GetMetricsAsync(int apartmentId, CancellationToken ct = default)
    {
        var apt = await _apartments.GetByIdAsync(apartmentId, ct)
            ?? throw new KeyNotFoundException($"Apartamento {apartmentId} no encontrado.");

        var reservations = (await _reservations.GetByApartmentAsync(apartmentId, ct)).ToList();

        var totalReservations = reservations.Count;
        var activeReservations = reservations.Count(r => r.IsActive);
        var totalRevenue = reservations
            .Where(r => r.Status != ReservationStatus.Cancelled)
            .Sum(r => r.TotalAmount);

        var completedReservations = reservations
            .Where(r => r.Status == ReservationStatus.CheckedOut)
            .ToList();

        var avgNightlyRate = completedReservations.Any()
            ? completedReservations.Average(r => r.TotalAmount / Math.Max(r.Nights, 1))
            : apt.BaseNightlyRate;

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var ninetyAgo = today.AddDays(-90);
        var occupiedDays = reservations
            .Where(r => r.Status != ReservationStatus.Cancelled
                     && r.CheckOutDate > ninetyAgo
                     && r.CheckInDate < today)
            .Sum(r =>
            {
                var start = r.CheckInDate < ninetyAgo ? ninetyAgo : r.CheckInDate;
                var end = r.CheckOutDate > today ? today : r.CheckOutDate;
                return Math.Max(0, end.DayNumber - start.DayNumber);
            });

        var occupancyRate = Math.Round((occupiedDays / 90.0) * 100, 1);

        var pendingIncidents = await _incidents.CountOpenByApartmentAsync(apartmentId, ct);

        return new ApartmentMetricsDto(
            apartmentId,
            apt.Name,
            apt.InternalCode,
            totalReservations,
            activeReservations,
            totalRevenue,
            avgNightlyRate,
            occupancyRate,
            PendingIncidents: pendingIncidents
        );
    }

    public async Task<ApartmentDto> CreateAsync(CreateApartmentRequest req, CancellationToken ct = default)
    {
        if (req.Bedrooms <= 0)
            throw new ArgumentException("El número de habitaciones debe ser mayor que 0.");

        if (req.MaxGuests <= 0)
            throw new ArgumentException("La capacidad máxima debe ser mayor que 0.");

        if (req.BaseNightlyRate <= 0)
            throw new ArgumentException("La tarifa base debe ser mayor que 0.");

        var owner = await _users.GetByIdAsync(req.OwnerId, ct)
            ?? throw new KeyNotFoundException($"Usuario {req.OwnerId} no encontrado.");

        if (owner.Role is not (UserRole.Owner or UserRole.Admin))
            throw new InvalidOperationException("El propietario debe tener rol Owner o Admin.");

        if (await _apartments.ExistsByCodeAsync(req.InternalCode, null, ct))
            throw new InvalidOperationException($"El código '{req.InternalCode}' ya está en uso.");

        var apartment = new Apartment
        {
            OwnerId = req.OwnerId,
            InternalCode = req.InternalCode.Trim().ToUpperInvariant(),
            Name = req.Name.Trim(),
            AddressLine = req.AddressLine.Trim(),
            District = req.District.Trim(),
            Bedrooms = req.Bedrooms,
            MaxGuests = req.MaxGuests,
            BaseNightlyRate = req.BaseNightlyRate,
            FloorArea = req.FloorArea,
            Latitude = req.Latitude,
            Longitude = req.Longitude,
            SmartLockCode = req.SmartLockCode?.Trim(),
            Status = ApartmentStatus.Active,
            CreatedAt = DateTime.UtcNow,
        };

        await _apartments.AddAsync(apartment, ct);
        await _apartments.SaveChangesAsync(ct);

        return await GetByIdAsync(apartment.Id, ct);
    }

    public async Task<ApartmentDto> UpdateAsync(int id, UpdateApartmentRequest req, CancellationToken ct = default)
    {
        var apt = await _apartments.GetByIdWithOwnerAsync(id, ct)
            ?? throw new KeyNotFoundException($"Apartamento {id} no encontrado.");

        if (req.Name is not null) apt.Name = req.Name.Trim();
        if (req.AddressLine is not null) apt.AddressLine = req.AddressLine.Trim();
        if (req.District is not null) apt.District = req.District.Trim();
        if (req.SmartLockCode is not null) apt.SmartLockCode = req.SmartLockCode.Trim();
        if (req.Latitude is not null) apt.Latitude = req.Latitude;
        if (req.Longitude is not null) apt.Longitude = req.Longitude;
        if (req.FloorArea is not null) apt.FloorArea = req.FloorArea;

        if (req.Bedrooms.HasValue)
        {
            if (req.Bedrooms.Value <= 0)
                throw new ArgumentException("El número de habitaciones debe ser mayor que 0.");
            apt.Bedrooms = req.Bedrooms.Value;
        }

        if (req.MaxGuests.HasValue)
        {
            if (req.MaxGuests.Value <= 0)
                throw new ArgumentException("La capacidad máxima debe ser mayor que 0.");
            apt.MaxGuests = req.MaxGuests.Value;
        }

        if (req.BaseNightlyRate.HasValue)
        {
            if (req.BaseNightlyRate.Value <= 0)
                throw new ArgumentException("La tarifa base debe ser mayor que 0.");
            apt.BaseNightlyRate = req.BaseNightlyRate.Value;
        }

        _apartments.Update(apt);
        await _apartments.SaveChangesAsync(ct);

        return ToDto(apt);
    }

    public async Task<ApartmentDto> UpdateStatusAsync(int id, UpdateApartmentStatusRequest req, CancellationToken ct = default)
    {
        if (!IsValidStatus(req.Status))
            throw new ArgumentException($"Estado inválido: {req.Status}.");

        var apt = await _apartments.GetByIdWithOwnerAsync(id, ct)
            ?? throw new KeyNotFoundException($"Apartamento {id} no encontrado.");

        if (req.Status is ApartmentStatus.Inactive or ApartmentStatus.UnderMaintenance)
        {
            var activeRes = (await _reservations.GetByApartmentAsync(id, ct))
                .Any(r => r.IsActive);

            if (activeRes)
                throw new InvalidOperationException(
                    "No se puede desactivar un apartamento con reservas activas.");
        }

        apt.Status = req.Status;
        _apartments.Update(apt);
        await _apartments.SaveChangesAsync(ct);

        return ToDto(apt);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var apt = await _apartments.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Apartamento {id} no encontrado.");

        var hasActiveReservations = (await _reservations.GetByApartmentAsync(id, ct))
            .Any(r => r.IsActive);

        if (hasActiveReservations)
            throw new InvalidOperationException(
                "No se puede eliminar un apartamento con reservas activas. Cancélalas primero.");

        apt.Status = ApartmentStatus.Inactive;
        _apartments.Update(apt);
        await _apartments.SaveChangesAsync(ct);
    }

    private static bool IsValidStatus(string status) =>
        status is ApartmentStatus.Active
               or ApartmentStatus.Inactive
               or ApartmentStatus.UnderMaintenance;

    private static ApartmentDto ToDto(Apartment a) => new(
        a.Id,
        a.OwnerId,
        a.Owner.FullName,
        a.Owner.Email,
        a.InternalCode,
        a.Name,
        a.AddressLine,
        a.District,
        a.Bedrooms,
        a.MaxGuests,
        a.BaseNightlyRate,
        a.FloorArea,
        a.Latitude,
        a.Longitude,
        a.SmartLockCode,
        a.Status,
        a.CreatedAt
    );

    private static ApartmentSummaryDto ToSummary(Apartment a) => new(
        a.Id,
        a.InternalCode,
        a.Name,
        a.District,
        a.Bedrooms,
        a.MaxGuests,
        a.BaseNightlyRate,
        a.Status
    );
}