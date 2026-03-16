using ABAPpartment.Domain.Entities;

public class CleaningSchedule
{
    public int Id { get; set; }
    public int ApartmentId { get; set; }
    public int? ReservationId { get; set; }
    public int? AssignedToId { get; set; }
    public DateOnly ScheduledDate { get; set; }
    public TimeOnly ScheduledTime { get; set; }
    public string Type { get; set; } = "Checkout";
    public string Status { get; set; } = "Scheduled";
    public DateTime? CompletedAt { get; set; }
    public string? Notes { get; set; }

    public Apartment Apartment { get; set; } = null!;
    public Reservation? Reservation { get; set; }
    public User? AssignedTo { get; set; }
}