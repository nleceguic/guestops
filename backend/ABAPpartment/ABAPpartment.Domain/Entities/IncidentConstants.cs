namespace ABAPpartment.Domain.Entities;

public static class IncidentStatus
{
    public const string Open = "Open";
    public const string InProgress = "InProgress";
    public const string Resolved = "Resolved";
    public const string Closed = "Closed";

    public static readonly IReadOnlyList<string> All =
        new[] { Open, InProgress, Resolved, Closed };

    public static bool IsActive(string status) =>
        status is Open or InProgress;
}

public static class IncidentCategory
{
    public const string Maintenance = "Maintenance";
    public const string Cleaning = "Cleaning";
    public const string Complaint = "Complaint";
    public const string Other = "Other";

    public static readonly IReadOnlyList<string> All =
        new[] { Maintenance, Cleaning, Complaint, Other };
}

public static class IncidentPriority
{
    public const string Low = "Low";
    public const string Medium = "Medium";
    public const string High = "High";
    public const string Critical = "Critical";

    public static readonly IReadOnlyList<string> All =
        new[] { Low, Medium, High, Critical };
}