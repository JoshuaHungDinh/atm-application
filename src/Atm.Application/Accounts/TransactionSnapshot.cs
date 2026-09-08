using Atm.Domain.Accounts;

namespace Atm.Application.Accounts;

/// <summary>Read model for a single transaction-history entry.</summary>
public sealed record TransactionSnapshot(
    Guid Id,
    TransactionType Type,
    decimal Amount,
    decimal BalanceAfter,
    DateTimeOffset OccurredAt,
    string? Description);
