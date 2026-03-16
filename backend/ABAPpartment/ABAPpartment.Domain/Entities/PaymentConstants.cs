namespace ABAPpartment.Domain.Entities;

public static class PaymentType
{
    public const string Deposit = "Deposit";
    public const string Balance = "Balance";
    public const string Refund = "Refund";
    public const string Extra = "Extra";

    public static readonly IReadOnlyList<string> All =
        new[] { Deposit, Balance, Refund, Extra };
}

public static class PaymentMethod
{
    public const string Card = "Card";
    public const string BankTransfer = "BankTransfer";
    public const string Cash = "Cash";
    public const string Stripe = "Stripe";

    public static readonly IReadOnlyList<string> All =
        new[] { Card, BankTransfer, Cash, Stripe };
}

public static class PaymentStatus
{
    public const string Pending = "Pending";
    public const string Completed = "Completed";
    public const string Failed = "Failed";
    public const string Refunded = "Refunded";

    public static readonly IReadOnlyList<string> All =
        new[] { Pending, Completed, Failed, Refunded };
}