using Atm.Domain.Exceptions;

namespace Atm.Domain.Accounts;

/// <summary>
/// Moves funds between two accounts. This lives in the domain because the rules that make
/// a transfer valid — distinct accounts, both legs applied together — are domain rules,
/// not orchestration. It operates on aggregates the caller has already loaded; persisting
/// the result is the application layer's job.
/// </summary>
public sealed class MoneyTransferService
{
    /// <summary>Withdraws from one account and deposits the same amount into another.</summary>
    /// <exception cref="InvalidTransferException"><paramref name="from"/> and <paramref name="to"/> are the same account.</exception>
    /// <exception cref="InvalidAmountException"><paramref name="amount"/> is not a well-formed positive amount.</exception>
    /// <exception cref="InsufficientFundsException"><paramref name="amount"/> exceeds the balance of <paramref name="from"/>.</exception>
    public void Transfer(Account from, Account to, decimal amount, TimeProvider clock)
    {
        ArgumentNullException.ThrowIfNull(from);
        ArgumentNullException.ThrowIfNull(to);
        ArgumentNullException.ThrowIfNull(clock);

        if (from.Id == to.Id)
        {
            throw new InvalidTransferException("Cannot transfer to the same account.");
        }

        from.Withdraw(amount, clock, $"Transfer to {to.Name}");
        to.Deposit(amount, clock, $"Transfer from {from.Name}");
    }
}
