using ABAPpartment.Domain.Entities;

namespace ABAPpartment.Domain.Interfaces;

public interface IGuestMessageRepository
{
    Task<GuestMessage?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IEnumerable<GuestMessage>> GetByReservationAsync(int reservationId, CancellationToken ct = default);
    Task<IEnumerable<GuestMessage>> GetByGuestAsync(int guestId, CancellationToken ct = default);
    Task<IEnumerable<GuestMessage>> GetRecentAsync(int count = 20, CancellationToken ct = default);
    Task<IEnumerable<GuestMessage>> GetPendingHumanReplyAsync(CancellationToken ct = default);

    Task<IEnumerable<GuestMessage>> GetLowConfidenceAsync(decimal threshold = 60, CancellationToken ct = default);

    Task AddAsync(GuestMessage message, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}