namespace ABAPpartment.Application.DTOs.Apartments;

/// <summary>Datos para crear un nuevo apartamento.</summary>

public record CreateApartmentRequest(
    int OwnerId,
    string InternalCode,
    string Name,
    string AddressLine,
    string District,
    int Bedrooms,
    int MaxGuests,
    decimal BaseNightlyRate,
    decimal? FloorArea = null,
    decimal? Latitude = null,
    decimal? Longitude = null,
    string? SmartLockCode = null
);

/// <summary>Datos para actualizar un apartamento existente (todos opcionales).</summary>

public record UpdateApartmentRequest(
    string? Name = null,
    string? AddressLine = null,
    string? District = null,
    int? Bedrooms = null,
    int? MaxGuests = null,
    decimal? BaseNightlyRate = null,
    decimal? FloorArea = null,
    decimal? Latitude = null,
    decimal? Longitude = null,
    string? SmartLockCode = null
);

/// <summary>Datos para cambiar el estado de un apartamento.</summary>

public record UpdateApartmentStatusRequest(string Status);

/// <summary>Apartamento completo con datos del propietario.</summary>

public record ApartmentDto(
    int Id,
    int OwnerId,
    string OwnerFullName,
    string OwnerEmail,
    string InternalCode,
    string Name,
    string AddressLine,
    string District,
    int Bedrooms,
    int MaxGuests,
    decimal BaseNightlyRate,
    decimal? FloorArea,
    decimal? Latitude,
    decimal? Longitude,
    string? SmartLockCode,
    string Status,
    DateTime CreatedAt
);

/// <summary>Versión resumida para listados.</summary>
public record ApartmentSummaryDto(
    int Id,
    string InternalCode,
    string Name,
    string District,
    int Bedrooms,
    int MaxGuests,
    decimal BaseNightlyRate,
    string Status
);

/// <summary>Métricas de rendimiento para el Owner Dashboard.</summary>

public record ApartmentMetricsDto(
    int ApartmentId,
    string Name,
    string InternalCode,
    int TotalReservations,
    int ActiveReservations,
    decimal TotalRevenue,
    decimal AverageNightlyRate,
    double OccupancyRatePercent,
    int PendingIncidents
);