using Atm.Domain.Accounts;

namespace Atm.Application.Accounts;

/// <summary>Read model for an account and its history, returned by the handlers.</summary>
public sealed record AccountSnapshot(
    Guid Id,
    string Name,
    decimal Balance,
    IReadOnlyList<TransactionSnapshot> Transactions)
{
    public static AccountSnapshot From(Account account)
    {
        ArgumentNullException.ThrowIfNull(account);

        TransactionSnapshot[] history = account.Transactions
            .Select(t => new TransactionSnapshot(
                t.Id, t.Type, t.Amount, t.BalanceAfter, t.OccurredAt, t.Description))
            .ToArray();

        return new AccountSnapshot(account.Id, account.Name, account.Balance, history);
    }
}
