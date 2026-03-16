using ABAPpartment.Application.DTOs.Reservations;
using ABAPpartment.Application.Interfaces;
using ABAPpartment.Domain.Entities;
using ABAPpartment.Domain.Interfaces;

namespace ABAPpartment.Application.Services;

public class ReservationService : IReservationService
{
    private readonly IReservationRepository _reservations;
    private readonly IApartmentRepository _apartments;
    private readonly IUserRepository _users;

    public ReservationService(
        IReservationRepository reservations,
        IApartmentRepository apartments,
        IUserRepository users)
    {
        _reservations = reservations;
        _apartments = apartments;
        _users = users;
    }

    public async Task<ReservationDto> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var r = await _reservations.GetByIdWithDetailsAsync(id, ct)
            ?? throw new KeyNotFoundException($"Reserva {id} no encontrada.");

        return ToDto(r);
    }

    public async Task<IEnumerable<ReservationSummaryDto>> GetAllAsync(CancellationToken ct = default)
    {
        var list = await _reservations.GetAllAsync(ct);
        return list.Select(ToSummary);
    }

    public async Task<IEnumerable<ReservationSummaryDto>> GetByApartmentAsync(int apartmentId, CancellationToken ct = default)
    {
        var list = await _reservations.GetByApartmentAsync(apartmentId, ct);
        return list.Select(ToSummary);
    }

    public async Task<IEnumerable<ReservationSummaryDto>> GetByGuestAsync(int guestId, CancellationToken ct = default)
    {
        var list = await _reservations.GetByGuestAsync(guestId, ct);
        return list.Select(ToSummary);
    }

    public async Task<ReservationDto> CreateAsync(
        int guestId,
        CreateReservationRequest req,
        CancellationToken ct = default)
    {
        if (req.CheckOutDate <= req.CheckInDate)
            throw new ArgumentException("La fecha de salida debe ser posterior a la de entrada.");

        if (req.NumGuests <= 0)
            throw new ArgumentException("El número de huéspedes debe ser mayor que 0.");

        var apartment = await _apartments.GetByIdAsync(req.ApartmentId, ct)
            ?? throw new KeyNotFoundException($"Apartamento {req.ApartmentId} no encontrado.");

        if (apartment.Status != ApartmentStatus.Active)
            throw new InvalidOperationException($"El apartamento '{apartment.Name}' no está disponible.");

        if (req.NumGuests > apartment.MaxGuests)
            throw new InvalidOperationException(
                $"El apartamento tiene capacidad máxima de {apartment.MaxGuests} huéspedes.");

        var hasOverlap = await _reservations.HasOverlapAsync(
            req.ApartmentId, req.CheckInDate, req.CheckOutDate, null, ct);

        if (hasOverlap)
            throw new InvalidOperationException(
                "El apartamento ya tiene una reserva en ese periodo.");

        var nights = req.CheckOutDate.DayNumber - req.CheckInDate.DayNumber;
        var totalAmount = apartment.BaseNightlyRate * nights;

        var reservation = new Reservation
        {
            ApartmentId = req.ApartmentId,
            GuestId = guestId,
            Channel = req.Channel,
            ExternalRef = req.ExternalRef,
            CheckInDate = req.CheckInDate,
            CheckOutDate = req.CheckOutDate,
            NumGuests = req.NumGuests,
            TotalAmount = totalAmount,
            Currency = "EUR",
            Status = ReservationStatus.Confirmed,
            CheckInMethod = req.CheckInMethod,
            SpecialRequests = req.SpecialRequests,
            CreatedAt = DateTime.UtcNow,
        };

        await _reservations.AddAsync(reservation, ct);
        await _reservations.SaveChangesAsync(ct);

        return await GetByIdAsync(reservation.Id, ct);
    }

    public async Task<ReservationDto> UpdateAsync(
        int id,
        UpdateReservationRequest req,
        CancellationToken ct = default)
    {
        var reservation = await _reservations.GetByIdWithDetailsAsync(id, ct)
            ?? throw new KeyNotFoundException($"Reserva {id} no encontrada.");

        if (!reservation.IsActive)
            throw new InvalidOperationException("Solo se pueden modificar reservas activas.");

        var newCheckIn = req.CheckInDate ?? reservation.CheckInDate;
        var newCheckOut = req.CheckOutDate ?? reservation.CheckOutDate;

        if (newCheckOut <= newCheckIn)
            throw new ArgumentException("La fecha de salida debe ser posterior a la de entrada.");

        if (req.CheckInDate.HasValue || req.CheckOutDate.HasValue)
        {
            var hasOverlap = await _reservations.HasOverlapAsync(
                reservation.ApartmentId, newCheckIn, newCheckOut, id, ct);

            if (hasOverlap)
                throw new InvalidOperationException("El nuevo rango de fechas solapa con otra reserva.");

            reservation.CheckInDate = newCheckIn;
            reservation.CheckOutDate = newCheckOut;

            var nights = newCheckOut.DayNumber - newCheckIn.DayNumber;
            reservation.TotalAmount = reservation.Apartment.BaseNightlyRate * nights;
        }

        if (req.NumGuests.HasValue)
        {
            if (req.NumGuests.Value > reservation.Apartment.MaxGuests)
                throw new InvalidOperationException(
                    $"Capacidad máxima del apartamento: {reservation.Apartment.MaxGuests} huéspedes.");
            reservation.NumGuests = req.NumGuests.Value;
        }

        if (req.CheckInMethod is not null) reservation.CheckInMethod = req.CheckInMethod;
        if (req.SpecialRequests is not null) reservation.SpecialRequests = req.SpecialRequests;

        _reservations.Update(reservation);
        await _reservations.SaveChangesAsync(ct);

        return ToDto(reservation);
    }

    public async Task<ReservationDto> UpdateStatusAsync(
        int id,
        UpdateStatusRequest req,
        CancellationToken ct = default)
    {
        if (!ReservationStatus.All.Contains(req.Status))
            throw new ArgumentException($"Estado inválido: {req.Status}.");

        var reservation = await _reservations.GetByIdWithDetailsAsync(id, ct)
            ?? throw new KeyNotFoundException($"Reserva {id} no encontrada.");

        var (current, next) = (reservation.Status, req.Status);
        var allowed = IsTransitionAllowed(current, next);

        if (!allowed)
            throw new InvalidOperationException(
                $"No se puede cambiar el estado de '{current}' a '{next}'.");

        reservation.Status = next;
        _reservations.Update(reservation);
        await _reservations.SaveChangesAsync(ct);

        return ToDto(reservation);
    }

    public async Task CancelAsync(int id, CancellationToken ct = default)
    {
        var reservation = await _reservations.GetByIdWithDetailsAsync(id, ct)
            ?? throw new KeyNotFoundException($"Reserva {id} no encontrada.");

        if (reservation.Status == ReservationStatus.Cancelled)
            throw new InvalidOperationException("La reserva ya está cancelada.");

        if (reservation.Status == ReservationStatus.CheckedOut)
            throw new InvalidOperationException("No se puede cancelar una reserva ya completada.");

        reservation.Status = ReservationStatus.Cancelled;
        reservation.CancelledAt = DateTime.UtcNow;

        _reservations.Update(reservation);
        await _reservations.SaveChangesAsync(ct);
    }

    private static bool IsTransitionAllowed(string current, string next) =>
        (current, next) switch
        {
            (ReservationStatus.Confirmed, ReservationStatus.CheckedIn) => true,
            (ReservationStatus.Confirmed, ReservationStatus.Cancelled) => true,
            (ReservationStatus.CheckedIn, ReservationStatus.CheckedOut) => true,
            _ => false
        };

    private static ReservationDto ToDto(Reservation r) => new(
        r.Id,
        r.ApartmentId,
        r.Apartment.Name,
        r.Apartment.AddressLine,
        r.GuestId,
        r.Guest.FullName,
        r.Guest.Email,
        r.Channel,
        r.ExternalRef,
        r.CheckInDate,
        r.CheckOutDate,
        r.Nights,
        r.NumGuests,
        r.TotalAmount,
        r.Currency,
        r.Status,
        r.CheckInMethod,
        r.SpecialRequests,
        r.CreatedAt,
        r.CancelledAt
    );

    private static ReservationSummaryDto ToSummary(Reservation r) => new(
        r.Id,
        r.ApartmentId,
        r.Apartment.Name,
        r.Guest.FullName,
        r.CheckInDate,
        r.CheckOutDate,
        r.Nights,
        r.TotalAmount,
        r.Status,
        r.Channel
    );
}