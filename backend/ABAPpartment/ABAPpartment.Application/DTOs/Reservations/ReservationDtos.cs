namespace ABAPpartment.Application.DTOs.Reservations;

/// <summary>Datos para crear una nueva reserva.</summary>
public record CreateReservationRequest(
    int ApartmentId,
    DateOnly CheckInDate,
    DateOnly CheckOutDate,
    int NumGuests,
    string Channel = "Direct",
    string? ExternalRef = null,
    string? CheckInMethod = null,
    string? SpecialRequests = null
);

/// <summary>Datos para actualizar una reserva existente (todos opcionales).</summary>
public record UpdateReservationRequest(
    DateOnly? CheckInDate,
    DateOnly? CheckOutDate,
    int? NumGuests,
    string? CheckInMethod,
    string? SpecialRequests
);

/// <summary>Datos para cambiar el estado de una reserva.</summary>
public record UpdateStatusRequest(string Status);

/// <summary>Reserva completa con datos del apartamento y huésped.</summary>
public record ReservationDto(
    int Id,
    int ApartmentId,
    string ApartmentName,
    string ApartmentAddress,
    int GuestId,
    string GuestFullName,
    string GuestEmail,
    string Channel,
    string? ExternalRef,
    DateOnly CheckInDate,
    DateOnly CheckOutDate,
    int Nights,
    int NumGuests,
    decimal TotalAmount,
    string Currency,
    string Status,
    string? CheckInMethod,
    string? SpecialRequests,
    DateTime CreatedAt,
    DateTime? CancelledAt
);

/// <summary>Versión resumida para listados.</summary>
public record ReservationSummaryDto(
    int Id,
    int ApartmentId,
    string ApartmentName,
    string GuestFullName,
    DateOnly CheckInDate,
    DateOnly CheckOutDate,
    int Nights,
    decimal TotalAmount,
    string Status,
    string Channel
);