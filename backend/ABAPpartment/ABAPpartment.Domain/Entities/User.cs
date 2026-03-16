namespace ABAPpartment.Domain.Entities;

public class User
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string Role { get; set; }
    public string Language { get; set; } = "es";
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }

    public ICollection<Apartment> OwnedApartments { get; set; } = new List<Apartment>();
    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    public ICollection<Incident> ReportedIncidents { get; set; } = new List<Incident>();
    public ICollection<Incident> AssignedIncidents { get; set; } = new List<Incident>();

    public ICollection<GuestMessage> GuestMessages { get; set; } = new List<GuestMessage>();
    public ICollection<CleaningSchedule> CleaningSchedules { get; set; } = new List<CleaningSchedule>();

    public string FullName => $"{FirstName} {LastName}";
}