namespace ABAPpartment.Domain.Entities;

public class Reservation
{
    public int Id { get; set; }
    public int ApartmentId { get; set; }
    public int GuestId { get; set; }
    public string Channel { get; set; } = ReservationChannel.Direct;
    public string? ExternalRef { get; set; }
    public DateOnly CheckInDate { get; set; }
    public DateOnly CheckOutDate { get; set; }
    public int NumGuests { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "EUR";
    public string Status { get; set; } = ReservationStatus.Confirmed;
    public string? CheckInMethod { get; set; }
    public string? SpecialRequests { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CancelledAt { get; set; }

    public Apartment Apartment { get; set; } = null!;
    public User Guest { get; set; } = null!;
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public ICollection<Incident> Incidents { get; set; } = new List<Incident>();

    public ICollection<GuestMessage> GuestMessages { get; set; } = new List<GuestMessage>();

    public int Nights => CheckOutDate.DayNumber - CheckInDate.DayNumber;
    public bool IsActive => Status is ReservationStatus.Confirmed or ReservationStatus.CheckedIn;
}

public static class ReservationStatus
{
    public const string Confirmed = "Confirmed";
    public const string CheckedIn = "CheckedIn";
    public const string CheckedOut = "CheckedOut";
    public const string Cancelled = "Cancelled";

    public static readonly IReadOnlyList<string> All =
        new[] { Confirmed, CheckedIn, CheckedOut, Cancelled };
}

public static class ReservationChannel
{
    public const string Direct = "Direct";
    public const string Airbnb = "Airbnb";
    public const string Booking = "Booking";
    public const string Other = "Other";
}

public static class CheckInMethod
{
    public const string SmartLock = "SmartLock";
    public const string OfficePickup = "OfficePickup";
    public const string KeyBox = "KeyBox";
}