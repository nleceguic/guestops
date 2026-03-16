using ABAPpartment.Domain.Entities;

namespace ABAPpartment.Domain.Interfaces;

public interface IPaymentRepository
{
    Task<Payment?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Payment?> GetByIdWithDetailsAsync(int id, CancellationToken ct = default);
    Task<IEnumerable<Payment>> GetByReservationAsync(int reservationId, CancellationToken ct = default);
    Task<IEnumerable<Payment>> GetByStatusAsync(string status, CancellationToken ct = default);
    Task<IEnumerable<Payment>> GetPendingAsync(CancellationToken ct = default);

    Task<decimal> GetTotalPaidAsync(int reservationId, CancellationToken ct = default);

    Task AddAsync(Payment payment, CancellationToken ct = default);
    void Update(Payment payment);
    Task SaveChangesAsync(CancellationToken ct = default);
}