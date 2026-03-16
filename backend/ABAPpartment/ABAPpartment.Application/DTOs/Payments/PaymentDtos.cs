namespace ABAPpartment.Application.DTOs.Payments;

/// <summary>Datos para registrar un nuevo pago.</summary>

public record CreatePaymentRequest(
    int ReservationId,
    decimal Amount,
    string Type,
    string Method,
    string? TransactionRef = null
);

/// <summary>Confirmar un pago pendiente (ej: confirmación de transferencia).</summary>

public record ConfirmPaymentRequest(
    string? TransactionRef = null
);

/// <summary>Marcar un pago como fallido.</summary>
public record FailPaymentRequest(string Reason);

/// <summary>Procesar un reembolso sobre un pago completado.</summary>

public record RefundPaymentRequest(
    decimal Amount,
    string? Reason = null
);

/// <summary>Pago completo con contexto de reserva.</summary>

public record PaymentDto(
    int Id,
    int ReservationId,
    string ApartmentName,
    string GuestFullName,
    decimal Amount,
    string Type,
    string Method,
    string Status,
    string? TransactionRef,
    DateTime? PaidAt,
    DateTime CreatedAt
);

/// <summary>Versión resumida para listados.</summary>
public record PaymentSummaryDto(
    int Id,
    int ReservationId,
    string ApartmentName,
    decimal Amount,
    string Type,
    string Method,
    string Status,
    DateTime? PaidAt
);

/// <summary>Resumen financiero de una reserva.</summary>
public record ReservationPaymentSummaryDto(
    int ReservationId,
    string ApartmentName,
    string GuestFullName,
    decimal TotalAmount,
    decimal TotalPaid,
    decimal PendingAmount,
    bool IsFullyPaid,
    IEnumerable<PaymentDto> Payments
);