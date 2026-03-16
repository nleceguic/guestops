namespace ABAPpartment.Domain.Entities;

public class GuestMessage
{
    public int Id { get; set; }
    public int? ReservationId { get; set; }
    public int GuestId { get; set; }
    public string Channel { get; set; } = string.Empty;
    public string Direction { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public bool IsAutoReply { get; set; }
    public decimal? AIConfidence { get; set; }
    public string? DetectedTopic { get; set; }
    public int? IncidentId { get; set; }
    public DateTime SentAt { get; set; }

    public User Guest { get; set; } = null!;
    public Reservation? Reservation { get; set; }
}