using ABAPpartment.Application.Services;
using ABAPpartment.Domain.Entities;
using ABAPpartment.Domain.Interfaces;
using ABAPpartment.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ABAPpartment.Infrastructure.Repositories;

public class GuestMessageRepository : IGuestMessageRepository
{
    private readonly AppDbContext _db;

    public GuestMessageRepository(AppDbContext db) => _db = db;

    private IQueryable<GuestMessage> WithDetails()
        => _db.GuestMessages
              .Include(m => m.Guest);

    public Task<GuestMessage?> GetByIdAsync(int id, CancellationToken ct = default)
        => WithDetails().FirstOrDefaultAsync(m => m.Id == id, ct);

    public async Task<IEnumerable<GuestMessage>> GetByReservationAsync(
        int reservationId, CancellationToken ct = default)
        => await WithDetails()
                 .Where(m => m.ReservationId == reservationId)
                 .AsNoTracking()
                 .OrderBy(m => m.SentAt)
                 .ToListAsync(ct);

    public async Task<IEnumerable<GuestMessage>> GetByGuestAsync(
        int guestId, CancellationToken ct = default)
        => await WithDetails()
                 .Where(m => m.GuestId == guestId)
                 .AsNoTracking()
                 .OrderByDescending(m => m.SentAt)
                 .ToListAsync(ct);

    public async Task<IEnumerable<GuestMessage>> GetRecentAsync(
        int count = 20, CancellationToken ct = default)
        => await WithDetails()
                 .Where(m => m.Direction == GuestMessageDirection.Inbound)
                 .AsNoTracking()
                 .OrderByDescending(m => m.SentAt)
                 .Take(count)
                 .ToListAsync(ct);

    public async Task<IEnumerable<GuestMessage>> GetPendingHumanReplyAsync(
        CancellationToken ct = default)
        => await WithDetails()
                 .Where(m => m.Direction == GuestMessageDirection.Inbound
                          && m.IsAutoReply == false
                          && !_db.GuestMessages.Any(r =>
                                r.ReservationId == m.ReservationId
                             && r.Direction == GuestMessageDirection.Outbound
                             && r.SentAt >= m.SentAt))
                 .AsNoTracking()
                 .OrderBy(m => m.SentAt)
                 .ToListAsync(ct);

    public async Task<IEnumerable<GuestMessage>> GetLowConfidenceAsync(
        decimal threshold = 60, CancellationToken ct = default)
        => await WithDetails()
                 .Where(m => m.IsAutoReply
                          && m.AIConfidence != null
                          && m.AIConfidence < threshold)
                 .AsNoTracking()
                 .OrderByDescending(m => m.SentAt)
                 .ToListAsync(ct);

    public async Task AddAsync(GuestMessage message, CancellationToken ct = default)
        => await _db.GuestMessages.AddAsync(message, ct);

    public Task SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);
}