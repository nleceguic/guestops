namespace ABAPpartment.Domain.Entities;

public class Apartment
{
    public int Id { get; set; }
    public int OwnerId { get; set; }
    public string InternalCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string AddressLine { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public int Bedrooms { get; set; }
    public int MaxGuests { get; set; }
    public decimal? FloorArea { get; set; }
    public decimal BaseNightlyRate { get; set; }
    public string? SmartLockCode { get; set; }
    public string Status { get; set; } = ApartmentStatus.Active;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User Owner { get; set; } = null!;
    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    public ICollection<Incident> Incidents { get; set; } = new List<Incident>();
    public ICollection<CleaningSchedule> CleaningSchedules { get; set; } = new List<CleaningSchedule>();
}

public static class ApartmentStatus
{
    public const string Active = "Active";
    public const string Inactive = "Inactive";
    public const string UnderMaintenance = "UnderMaintenance";
}