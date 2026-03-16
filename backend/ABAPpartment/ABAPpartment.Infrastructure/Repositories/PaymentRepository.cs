using ABAPpartment.Domain.Entities;
using ABAPpartment.Domain.Interfaces;
using ABAPpartment.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ABAPpartment.Infrastructure.Repositories;

public class PaymentRepository : IPaymentRepository
{
    private readonly AppDbContext _db;

    public PaymentRepository(AppDbContext db) => _db = db;

    private IQueryable<Payment> WithDetails()
        => _db.Payments
              .Include(p => p.Reservation)
                  .ThenInclude(r => r.Apartment)
              .Include(p => p.Reservation)
                  .ThenInclude(r => r.Guest);

    public Task<Payment?> GetByIdAsync(int id, CancellationToken ct = default)
        => _db.Payments.FirstOrDefaultAsync(p => p.Id == id, ct);

    public Task<Payment?> GetByIdWithDetailsAsync(int id, CancellationToken ct = default)
        => WithDetails().FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<IEnumerable<Payment>> GetByReservationAsync(
        int reservationId,
        CancellationToken ct = default)
        => await WithDetails()
                 .Where(p => p.ReservationId == reservationId)
                 .AsNoTracking()
                 .OrderBy(p => p.CreatedAt)
                 .ToListAsync(ct);

    public async Task<IEnumerable<Payment>> GetByStatusAsync(
        string status,
        CancellationToken ct = default)
        => await WithDetails()
                 .Where(p => p.Status == status)
                 .AsNoTracking()
                 .OrderByDescending(p => p.CreatedAt)
                 .ToListAsync(ct);

    public async Task<IEnumerable<Payment>> GetPendingAsync(CancellationToken ct = default)
        => await WithDetails()
                 .Where(p => p.Status == PaymentStatus.Pending)
                 .AsNoTracking()
                 .OrderBy(p => p.CreatedAt)
                 .ToListAsync(ct);

    public Task<decimal> GetTotalPaidAsync(int reservationId, CancellationToken ct = default)
        => _db.Payments
              .Where(p => p.ReservationId == reservationId
                       && p.Status == PaymentStatus.Completed)
              .SumAsync(p => p.Amount, ct);

    public async Task AddAsync(Payment payment, CancellationToken ct = default)
        => await _db.Payments.AddAsync(payment, ct);

    public void Update(Payment payment)
        => _db.Payments.Update(payment);

    public Task SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);
}