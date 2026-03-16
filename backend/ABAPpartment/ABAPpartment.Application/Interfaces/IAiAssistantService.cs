namespace ABAPpartment.Application.Interfaces;
public interface IAIAssistantService
{
    Task<AIAssistantResponse?> GetResponseAsync(
        string guestMessage,
        string guestLanguage,
        AIAssistantContext context,
        CancellationToken ct = default);
}

public record AIAssistantResponse(
    string Reply,
    decimal Confidence,
    string DetectedTopic,
    bool ShouldCreateIncident
);

public record AIAssistantContext(
    string ApartmentName,
    string ApartmentAddress,
    string ApartmentDistrict,
    string? SmartLockCode,
    DateOnly CheckInDate,
    DateOnly CheckOutDate,
    string GuestFirstName
);