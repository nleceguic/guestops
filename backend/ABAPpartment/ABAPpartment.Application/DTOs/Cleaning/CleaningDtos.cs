namespace ABAPpartment.Application.DTOs.Cleaning;

/// <summary>Datos para crear una limpieza manualmente.</summary>

public record CreateCleaningScheduleRequest(
    int ApartmentId,
    DateOnly ScheduledDate,
    TimeOnly ScheduledTime,
    string Type,
    int? ReservationId = null,
    int? AssignedToId = null,
    string? Notes = null
);

/// <summary>Datos para actualizar una limpieza.</summary>

public record UpdateCleaningScheduleRequest(
    DateOnly? ScheduledDate = null,
    TimeOnly? ScheduledTime = null,
    int? AssignedToId = null,
    string? Notes = null
);

/// <summary>Datos para cambiar el estado de una limpieza.</summary>

public record UpdateCleaningStatusRequest(
    string Status,
    string? Notes = null
);

/// <summary>Limpieza completa con contexto del apartamento y operario.</summary>

public record CleaningScheduleDto(
    int Id,
    int ApartmentId,
    string ApartmentName,
    string ApartmentDistrict,
    int? ReservationId,
    int? AssignedToId,
    string? AssignedToFullName,
    DateOnly ScheduledDate,
    TimeOnly ScheduledTime,
    string Type,
    string Status,
    DateTime? CompletedAt,
    string? Notes
);

/// <summary>Versión compacta para el planning diario del Operations Dashboard.</summary>
public record CleaningScheduleSummaryDto(
    int Id,
    string ApartmentName,
    string ApartmentDistrict,
    string? AssignedToFullName,
    DateOnly ScheduledDate,
    TimeOnly ScheduledTime,
    string Type,
    string Status
);

/// <summary>Resumen del planning de un día para el Operations Dashboard.</summary>
public record DailyPlanningDto(
    DateOnly Date,
    int Total,
    int Scheduled,
    int InProgress,
    int Done,
    int Skipped,
    IEnumerable<CleaningScheduleSummaryDto> Cleanings
);