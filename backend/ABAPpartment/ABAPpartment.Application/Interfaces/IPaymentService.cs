using ABAPpartment.Application.DTOs.Payments;

namespace ABAPpartment.Application.Interfaces;

public interface IPaymentService
{
    Task<PaymentDto> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IEnumerable<PaymentSummaryDto>> GetPendingAsync(CancellationToken ct = default);
    Task<IEnumerable<PaymentSummaryDto>> GetByStatusAsync(string status, CancellationToken ct = default);
    Task<ReservationPaymentSummaryDto> GetByReservationAsync(int reservationId, CancellationToken ct = default);
    Task<PaymentDto> CreateAsync(CreatePaymentRequest request, CancellationToken ct = default);
    Task<PaymentDto> ConfirmAsync(int id, ConfirmPaymentRequest request, CancellationToken ct = default);
    Task<PaymentDto> FailAsync(int id, FailPaymentRequest request, CancellationToken ct = default);
    Task<PaymentDto> RefundAsync(int id, RefundPaymentRequest request, CancellationToken ct = default);
}