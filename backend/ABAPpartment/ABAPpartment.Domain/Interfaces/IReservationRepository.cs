using ABAPpartment.Domain.Entities;

namespace ABAPpartment.Domain.Interfaces;

public interface IReservationRepository
{
    Task<Reservation?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Reservation?> GetByIdWithDetailsAsync(int id, CancellationToken ct = default);
    Task<IEnumerable<Reservation>> GetAllAsync(CancellationToken ct = default);
    Task<IEnumerable<Reservation>> GetByApartmentAsync(int apartmentId, CancellationToken ct = default);
    Task<IEnumerable<Reservation>> GetByGuestAsync(int guestId, CancellationToken ct = default);
    Task<bool> HasOverlapAsync(
        int apartmentId,
        DateOnly checkIn,
        DateOnly checkOut,
        int? excludeReservationId = null,
        CancellationToken ct = default);

    Task AddAsync(Reservation reservation, CancellationToken ct = default);
    void Update(Reservation reservation);
    Task SaveChangesAsync(CancellationToken ct = default);
}