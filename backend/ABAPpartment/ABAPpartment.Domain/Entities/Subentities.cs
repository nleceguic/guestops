namespace ABAPpartment.Domain.Entities;

public class Payment
{
    public int Id { get; set; }
    public int ReservationId { get; set; }
    public decimal Amount { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";
    public string? TransactionRef { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Reservation Reservation { get; set; } = null!;
}

public class Incident
{
    public int Id { get; set; }
    public int ApartmentId { get; set; }
    public int? ReservationId { get; set; }
    public int ReportedById { get; set; }
    public int? AssignedToId { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Priority { get; set; } = "Medium";
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = "Open";
    public string? ZendeskTicketId { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Apartment Apartment { get; set; } = null!;
    public Reservation? Reservation { get; set; }
    public User ReportedBy { get; set; } = null!;
    public User? AssignedTo { get; set; }
}

public class OccupancyForecast
{
    public int Id { get; set; }
    public int ApartmentId { get; set; }
    public DateOnly ForecastDate { get; set; }
    public decimal PredictedRate { get; set; }
    public decimal? SuggestedPrice { get; set; }
    public string ModelVersion { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

public class AuditLog
{
    public long Id { get; set; }
    public int? UserId { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public int EntityId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public string? IPAddress { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public User? User { get; set; }
}