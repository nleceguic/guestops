namespace ABAPpartment.Domain.Entities;

public static class UserRole
{
    public const string Guest = "Guest";
    public const string Owner = "Owner";
    public const string Operator = "Operator";
    public const string Admin = "Admin";

    public static readonly IReadOnlyList<string> All =
        new[] { Guest, Owner, Operator, Admin };

    public static bool IsValid(string role) => All.Contains(role);
}