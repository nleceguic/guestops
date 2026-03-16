using ABAPpartment.Application.DTOs.Payments;
using ABAPpartment.Application.Interfaces;
using ABAPpartment.Domain.Entities;
using ABAPpartment.Domain.Interfaces;

namespace ABAPpartment.Application.Services;

public class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _payments;
    private readonly IReservationRepository _reservations;

    public PaymentService(
        IPaymentRepository payments,
        IReservationRepository reservations)
    {
        _payments = payments;
        _reservations = reservations;
    }

    public async Task<PaymentDto> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var payment = await _payments.GetByIdWithDetailsAsync(id, ct)
            ?? throw new KeyNotFoundException($"Pago {id} no encontrado.");
        return ToDto(payment);
    }

    public async Task<IEnumerable<PaymentSummaryDto>> GetPendingAsync(CancellationToken ct = default)
        => (await _payments.GetPendingAsync(ct)).Select(ToSummary);

    public async Task<IEnumerable<PaymentSummaryDto>> GetByStatusAsync(string status, CancellationToken ct = default)
    {
        if (!PaymentStatus.All.Contains(status))
            throw new ArgumentException($"Estado inválido: {status}.");
        return (await _payments.GetByStatusAsync(status, ct)).Select(ToSummary);
    }

    public async Task<ReservationPaymentSummaryDto> GetByReservationAsync(
        int reservationId,
        CancellationToken ct = default)
    {
        var reservation = await _reservations.GetByIdWithDetailsAsync(reservationId, ct)
            ?? throw new KeyNotFoundException($"Reserva {reservationId} no encontrada.");

        var payments = (await _payments.GetByReservationAsync(reservationId, ct)).ToList();
        var effectivePaid = payments
            .Where(p => p.Status == PaymentStatus.Completed
                     || p.Status == PaymentStatus.Refunded)
            .Sum(p => p.Amount);
        var pendingAmount = Math.Max(0, reservation.TotalAmount - effectivePaid);

        return new ReservationPaymentSummaryDto(
            ReservationId: reservationId,
            ApartmentName: reservation.Apartment.Name,
            GuestFullName: reservation.Guest.FullName,
            TotalAmount: reservation.TotalAmount,
            TotalPaid: effectivePaid,
            PendingAmount: pendingAmount,
            IsFullyPaid: pendingAmount == 0,
            Payments: payments.Select(ToDto)
        );
    }

    public async Task<PaymentDto> CreateAsync(
        CreatePaymentRequest req,
        CancellationToken ct = default)
    {
        if (!PaymentType.All.Contains(req.Type))
            throw new ArgumentException($"Tipo de pago inválido: {req.Type}.");

        if (!PaymentMethod.All.Contains(req.Method))
            throw new ArgumentException($"Método de pago inválido: {req.Method}.");

        if (req.Amount == 0)
            throw new ArgumentException("El importe no puede ser 0.");

        if (req.Type == PaymentType.Refund && req.Amount > 0)
            throw new ArgumentException("Los reembolsos deben tener importe negativo.");

        if (req.Type != PaymentType.Refund && req.Amount < 0)
            throw new ArgumentException("El importe debe ser positivo.");

        var reservation = await _reservations.GetByIdWithDetailsAsync(req.ReservationId, ct)
            ?? throw new KeyNotFoundException($"Reserva {req.ReservationId} no encontrada.");

        if (reservation.Status == ReservationStatus.Cancelled)
            throw new InvalidOperationException(
                "No se pueden añadir pagos a una reserva cancelada.");

        if (req.Type is PaymentType.Deposit or PaymentType.Balance)
        {
            var totalPaid = await _payments.GetTotalPaidAsync(req.ReservationId, ct);
            if (totalPaid + req.Amount > reservation.TotalAmount)
                throw new InvalidOperationException(
                    $"El pago supera el importe pendiente. " +
                    $"Total reserva: {reservation.TotalAmount}€, " +
                    $"ya pagado: {totalPaid}€.");
        }

        var payment = new Payment
        {
            ReservationId = req.ReservationId,
            Amount = req.Amount,
            Type = req.Type,
            Method = req.Method,
            Status = PaymentStatus.Pending,
            TransactionRef = req.TransactionRef?.Trim(),
            CreatedAt = DateTime.UtcNow,
        };

        if (req.Method == PaymentMethod.Cash)
        {
            payment.Status = PaymentStatus.Completed;
            payment.PaidAt = DateTime.UtcNow;
        }

        await _payments.AddAsync(payment, ct);
        await _payments.SaveChangesAsync(ct);

        return await GetByIdAsync(payment.Id, ct);
    }

    public async Task<PaymentDto> ConfirmAsync(
        int id,
        ConfirmPaymentRequest req,
        CancellationToken ct = default)
    {
        var payment = await _payments.GetByIdWithDetailsAsync(id, ct)
            ?? throw new KeyNotFoundException($"Pago {id} no encontrado.");

        if (payment.Status != PaymentStatus.Pending)
            throw new InvalidOperationException(
                $"Solo se pueden confirmar pagos en estado Pending. Estado actual: {payment.Status}.");

        payment.Status = PaymentStatus.Completed;
        payment.PaidAt = DateTime.UtcNow;

        if (req.TransactionRef is not null)
            payment.TransactionRef = req.TransactionRef.Trim();

        _payments.Update(payment);
        await _payments.SaveChangesAsync(ct);

        return ToDto(payment);
    }

    public async Task<PaymentDto> FailAsync(
        int id,
        FailPaymentRequest req,
        CancellationToken ct = default)
    {
        var payment = await _payments.GetByIdWithDetailsAsync(id, ct)
            ?? throw new KeyNotFoundException($"Pago {id} no encontrado.");

        if (payment.Status != PaymentStatus.Pending)
            throw new InvalidOperationException(
                "Solo se pueden marcar como fallidos pagos en estado Pending.");

        payment.Status = PaymentStatus.Failed;
        payment.TransactionRef = $"[FAILED]: {req.Reason}";

        _payments.Update(payment);
        await _payments.SaveChangesAsync(ct);

        return ToDto(payment);
    }

    public async Task<PaymentDto> RefundAsync(
        int id,
        RefundPaymentRequest req,
        CancellationToken ct = default)
    {
        var original = await _payments.GetByIdWithDetailsAsync(id, ct)
            ?? throw new KeyNotFoundException($"Pago {id} no encontrado.");

        if (original.Status != PaymentStatus.Completed)
            throw new InvalidOperationException(
                "Solo se pueden reembolsar pagos completados.");

        if (req.Amount <= 0)
            throw new ArgumentException("El importe del reembolso debe ser mayor que 0.");

        if (req.Amount > original.Amount)
            throw new InvalidOperationException(
                $"El reembolso ({req.Amount}€) no puede superar el pago original ({original.Amount}€).");

        var refund = new Payment
        {
            ReservationId = original.ReservationId,
            Amount = -req.Amount,
            Type = PaymentType.Refund,
            Method = original.Method,
            Status = PaymentStatus.Refunded,
            TransactionRef = req.Reason is not null
                             ? $"Reembolso de pago #{id}: {req.Reason}"
                             : $"Reembolso de pago #{id}",
            PaidAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
        };

        if (req.Amount == original.Amount)
            original.Status = PaymentStatus.Refunded;

        _payments.Update(original);
        await _payments.AddAsync(refund, ct);
        await _payments.SaveChangesAsync(ct);

        return await GetByIdAsync(refund.Id, ct);
    }

    private static PaymentDto ToDto(Payment p) => new(
        p.Id,
        p.ReservationId,
        p.Reservation.Apartment.Name,
        p.Reservation.Guest.FullName,
        p.Amount,
        p.Type,
        p.Method,
        p.Status,
        p.TransactionRef,
        p.PaidAt,
        p.CreatedAt
    );

    private static PaymentSummaryDto ToSummary(Payment p) => new(
        p.Id,
        p.ReservationId,
        p.Reservation.Apartment.Name,
        p.Amount,
        p.Type,
        p.Method,
        p.Status,
        p.PaidAt
    );
}