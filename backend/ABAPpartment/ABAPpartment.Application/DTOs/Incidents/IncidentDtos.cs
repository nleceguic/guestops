namespace ABAPpartment.Application.DTOs.Incidents;

/// <summary>Datos para abrir una nueva incidencia.</summary>
public record CreateIncidentRequest(
    int ApartmentId,
    string Category,
    string Priority,
    string Title,
    string? Description = null,
    int? ReservationId = null
);

/// <summary>Datos para actualizar una incidencia.</summary>

public record UpdateIncidentRequest(
    string? Title = null,
    string? Description = null,
    string? Priority = null,
    string? Category = null
);

/// <summary>Asignar un operario a una incidencia.</summary>

public record AssignIncidentRequest(int AssignedToId);

/// <summary>Cambiar el estado de una incidencia.</summary>

public record UpdateIncidentStatusRequest(
    string Status,
    string? ResolutionNote = null
);

/// <summary>Sincronizar el ticket de Zendesk.</summary>

public record SyncZendeskRequest(string ZendeskTicketId);

/// <summary>Incidencia completa con contexto de apartamento, reserva y operario.</summary>

public record IncidentDto(
    int Id,
    int ApartmentId,
    string ApartmentName,
    string ApartmentDistrict,
    int? ReservationId,
    int ReportedById,
    string ReportedByFullName,
    int? AssignedToId,
    string? AssignedToFullName,
    string Category,
    string Priority,
    string Title,
    string? Description,
    string Status,
    string? ZendeskTicketId,
    DateTime? ResolvedAt,
    DateTime CreatedAt
);

/// <summary>Versión resumida para listados y Operations Dashboard.</summary>
public record IncidentSummaryDto(
    int Id,
    string ApartmentName,
    string Category,
    string Priority,
    string Title,
    string? AssignedToFullName,
    string Status,
    string? ZendeskTicketId,
    DateTime CreatedAt
);

/// <summary>Resumen global de incidencias para el Operations Dashboard.</summary>
public record IncidentDashboardDto(
    int TotalOpen,
    int TotalInProgress,
    int TotalResolved,
    int CriticalOpen,
    int HighOpen,
    int UnassignedOpen,
    IEnumerable<IncidentSummaryDto> CriticalAndHigh,
    IEnumerable<IncidentSummaryDto> RecentlyOpened
);