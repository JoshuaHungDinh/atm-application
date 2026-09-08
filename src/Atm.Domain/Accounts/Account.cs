using Atm.Domain.Exceptions;

namespace Atm.Domain.Accounts;

/// <summary>
/// A bank account and the aggregate root of the domain: it owns its balance and its
/// transaction history and is the only place the invariants are enforced. Cross-account
/// transfers live in the application layer as a Withdraw plus a Deposit.
/// </summary>
public sealed class Account
{
    private readonly List<Transaction> _transactions = [];

    public Account(Guid id, string name, decimal openingBalance = 0m)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Account id must not be empty.", nameof(id));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Id = id;
        Name = name;
        Balance = ValidateAmount(openingBalance, allowZero: true);
    }

    public Guid Id { get; }

    public string Name { get; }

    public decimal Balance { get; private set; }

    /// <summary>
    /// The account's history, oldest first. Each access returns an independent snapshot:
    /// callers cannot mutate it, nor is it affected by later movements.
    /// </summary>
    public IReadOnlyList<Transaction> Transactions => _transactions.ToArray();

    /// <summary>Adds funds to the account and records the movement.</summary>
    /// <exception cref="InvalidAmountException"><paramref name="amount"/> is not a well-formed positive amount.</exception>
    public void Deposit(decimal amount, TimeProvider clock, string? description = null)
    {
        ArgumentNullException.ThrowIfNull(clock);

        decimal applied = ValidateAmount(amount);
        Balance += applied;
        Record(TransactionType.Deposit, applied, clock, description);
    }

    /// <summary>Removes funds from the account and records the movement.</summary>
    /// <exception cref="InvalidAmountException"><paramref name="amount"/> is not a well-formed positive amount.</exception>
    /// <exception cref="InsufficientFundsException"><paramref name="amount"/> exceeds the current balance.</exception>
    public void Withdraw(decimal amount, TimeProvider clock, string? description = null)
    {
        ArgumentNullException.ThrowIfNull(clock);

        decimal applied = ValidateAmount(amount);
        if (applied > Balance)
        {
            throw new InsufficientFundsException(Balance, applied);
        }

        Balance -= applied;
        Record(TransactionType.Withdrawal, applied, clock, description);
    }

    private void Record(TransactionType type, decimal amount, TimeProvider clock, string? description)
    {
        _transactions.Add(
            new Transaction(Guid.NewGuid(), type, amount, Balance, clock.GetUtcNow(), description));
    }

    private static decimal ValidateAmount(decimal amount, bool allowZero = false)
    {
        if (amount < 0m || (amount == 0m && !allowZero))
        {
            throw allowZero
                ? InvalidAmountException.MustNotBeNegative()
                : InvalidAmountException.MustBePositive();
        }

        if (decimal.Round(amount, 2) != amount)
        {
            throw InvalidAmountException.TooManyDecimalPlaces();
        }

        return amount;
    }
}
