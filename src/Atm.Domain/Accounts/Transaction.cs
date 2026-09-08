namespace Atm.Domain.Accounts;

/// <summary>
/// An immutable record of a single balance movement on an account. Amount is always
/// positive; Type gives it direction. Together, transactions form the account's history.
/// </summary>
public sealed class Transaction
{
    internal Transaction(
        Guid id,
        TransactionType type,
        decimal amount,
        decimal balanceAfter,
        DateTimeOffset occurredAt,
        string? description)
    {
        Id = id;
        Type = type;
        Amount = amount;
        BalanceAfter = balanceAfter;
        OccurredAt = occurredAt;
        Description = description;
    }

    public Guid Id { get; }

    public TransactionType Type { get; }

    public decimal Amount { get; }

    public decimal BalanceAfter { get; }

    public DateTimeOffset OccurredAt { get; }

    public string? Description { get; }
}
