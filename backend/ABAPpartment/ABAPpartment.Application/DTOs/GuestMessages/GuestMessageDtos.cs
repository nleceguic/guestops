namespace ABAPpartment.Application.DTOs.GuestMessages;

/// <summary>Mensaje entrante de un huésped (inbound).</summary>

public record ProcessMessageRequest(
    int GuestId,
    string Body,
    string Channel,
    int? ReservationId = null
);

/// <summary>Respuesta manual enviada por un operador (outbound).</summary>

public record SendManualReplyRequest(
    int OperatorId,
    string Body
);

/// <summary>Mensaje completo con metadatos de IA.</summary>

public record GuestMessageDto(
    int Id,
    int? ReservationId,
    int GuestId,
    string GuestFullName,
    string Channel,
    string Direction,
    string Body,
    bool IsAutoReply,
    decimal? AIConfidence,
    string? DetectedTopic,
    int? IncidentId,
    DateTime SentAt
);

/// <summary>Resultado completo del procesamiento de un mensaje inbound.</summary>

public record ProcessMessageResult(
    GuestMessageDto InboundMessage,
    GuestMessageDto? AutoReply,
    bool WasAutoReplied,
    bool IncidentCreated,
    int? IncidentId,
    string DetectedTopic,
    decimal? AIConfidence
);