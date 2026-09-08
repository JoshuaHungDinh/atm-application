using System.Globalization;

using Atm.Domain.Accounts;
using Atm.Domain.Exceptions;
using Atm.UnitTests.TestDoubles;

namespace Atm.UnitTests.Accounts;

/// <summary>
/// Behavioural tests for the <see cref="Account"/> aggregate: balance arithmetic, the
/// invariants it enforces, and the integrity of its transaction history.
/// </summary>
public class AccountTests
{
    private static readonly DateTimeOffset At = new(2026, 9, 7, 12, 0, 0, TimeSpan.Zero);
    private readonly FixedClock _clock = new(At);

    private static Account NewAccount(decimal openingBalance = 0m) =>
        new(Guid.NewGuid(), "Checking", openingBalance);

    private static decimal Money(string value) =>
        decimal.Parse(value, CultureInfo.InvariantCulture);

    [Fact]
    public void Deposit_increases_balance_and_records_a_transaction()
    {
        Account account = NewAccount(openingBalance: 100m);

        account.Deposit(40m, _clock);

        Assert.Equal(140m, account.Balance);
        Transaction entry = Assert.Single(account.Transactions);
        Assert.Equal(TransactionType.Deposit, entry.Type);
        Assert.Equal(40m, entry.Amount);
        Assert.Equal(140m, entry.BalanceAfter);
        Assert.Equal(At, entry.OccurredAt);
    }

    [Fact]
    public void Withdraw_decreases_balance_and_records_a_transaction()
    {
        Account account = NewAccount(openingBalance: 100m);

        account.Withdraw(30m, _clock);

        Assert.Equal(70m, account.Balance);
        Transaction entry = Assert.Single(account.Transactions);
        Assert.Equal(TransactionType.Withdrawal, entry.Type);
        Assert.Equal(30m, entry.Amount);
        Assert.Equal(70m, entry.BalanceAfter);
    }

    [Fact]
    public void Withdraw_of_the_entire_balance_leaves_zero()
    {
        Account account = NewAccount(openingBalance: 50m);

        account.Withdraw(50m, _clock);

        Assert.Equal(0m, account.Balance);
    }

    [Fact]
    public void Withdraw_more_than_the_balance_is_rejected_and_leaves_the_account_untouched()
    {
        Account account = NewAccount(openingBalance: 50m);

        InsufficientFundsException error =
            Assert.Throws<InsufficientFundsException>(() => account.Withdraw(50.01m, _clock));

        Assert.Equal(50m, error.Balance);
        Assert.Equal(50.01m, error.Requested);
        Assert.Equal(50m, account.Balance);
        Assert.Empty(account.Transactions);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("-0.01")]
    [InlineData("-100")]
    public void Deposit_of_a_non_positive_amount_is_rejected(string amount)
    {
        Account account = NewAccount(openingBalance: 100m);

        Assert.Throws<InvalidAmountException>(() => account.Deposit(Money(amount), _clock));
        Assert.Equal(100m, account.Balance);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("-0.01")]
    [InlineData("-100")]
    public void Withdraw_of_a_non_positive_amount_is_rejected(string amount)
    {
        Account account = NewAccount(openingBalance: 100m);

        Assert.Throws<InvalidAmountException>(() => account.Withdraw(Money(amount), _clock));
        Assert.Equal(100m, account.Balance);
    }

    [Theory]
    [InlineData("0.001")]
    [InlineData("1.234")]
    [InlineData("100.005")]
    public void Deposit_of_an_amount_with_sub_cent_precision_is_rejected(string amount)
    {
        Account account = NewAccount();

        Assert.Throws<InvalidAmountException>(() => account.Deposit(Money(amount), _clock));
    }

    [Theory]
    [InlineData("0.001")]
    [InlineData("1.234")]
    public void Withdraw_of_an_amount_with_sub_cent_precision_is_rejected(string amount)
    {
        Account account = NewAccount(openingBalance: 100m);

        Assert.Throws<InvalidAmountException>(() => account.Withdraw(Money(amount), _clock));
    }

    [Fact]
    public void Deposit_and_withdraw_reject_a_null_clock()
    {
        Account account = NewAccount(openingBalance: 100m);

        Assert.Throws<ArgumentNullException>(() => account.Deposit(10m, null!));
        Assert.Throws<ArgumentNullException>(() => account.Withdraw(10m, null!));
    }

    [Fact]
    public void Opening_balance_seeds_the_balance_without_recording_a_transaction()
    {
        Account account = NewAccount(openingBalance: 250m);

        Assert.Equal(250m, account.Balance);
        Assert.Empty(account.Transactions);
    }

    [Fact]
    public void A_negative_opening_balance_is_rejected()
    {
        Assert.Throws<InvalidAmountException>(() => NewAccount(openingBalance: -1m));
    }

    [Fact]
    public void An_opening_balance_with_sub_cent_precision_is_rejected()
    {
        Assert.Throws<InvalidAmountException>(() => NewAccount(openingBalance: 0.001m));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void A_blank_name_is_rejected(string? name)
    {
        Assert.ThrowsAny<ArgumentException>(() => new Account(Guid.NewGuid(), name!));
    }

    [Fact]
    public void An_empty_id_is_rejected()
    {
        Assert.Throws<ArgumentException>(() => new Account(Guid.Empty, "Checking"));
    }

    [Fact]
    public void History_is_ordered_oldest_first_with_a_running_balance_after()
    {
        Account account = NewAccount(openingBalance: 100m);

        account.Deposit(50m, _clock);
        account.Withdraw(30m, _clock);
        account.Deposit(10m, _clock);

        IReadOnlyList<Transaction> history = account.Transactions;
        Assert.Equal(3, history.Count);

        Assert.Equal(TransactionType.Deposit, history[0].Type);
        Assert.Equal(150m, history[0].BalanceAfter);

        Assert.Equal(TransactionType.Withdrawal, history[1].Type);
        Assert.Equal(120m, history[1].BalanceAfter);

        Assert.Equal(TransactionType.Deposit, history[2].Type);
        Assert.Equal(130m, history[2].BalanceAfter);

        Assert.Equal(130m, account.Balance);
    }

    [Fact]
    public void An_optional_description_is_stored_on_the_transaction()
    {
        Account account = NewAccount(openingBalance: 100m);

        account.Withdraw(25m, _clock, "Transfer to Savings");

        Assert.Equal("Transfer to Savings", Assert.Single(account.Transactions).Description);
    }

    [Fact]
    public void Transactions_returns_an_independent_snapshot_each_call()
    {
        Account account = NewAccount(openingBalance: 100m);
        account.Deposit(10m, _clock);

        IReadOnlyList<Transaction> first = account.Transactions;
        account.Deposit(20m, _clock);

        Assert.Single(first);
        Assert.Equal(2, account.Transactions.Count);
        Assert.NotSame(first, account.Transactions);
    }

    [Fact]
    public void The_internal_transaction_list_is_never_handed_to_callers()
    {
        Account account = NewAccount(openingBalance: 100m);
        account.Deposit(10m, _clock);

        // The aggregate's own mutable list is not what callers receive.
        Assert.Null(account.Transactions as List<Transaction>);

        // Overwriting the returned copy leaves the account's history intact.
        Transaction[] copy = (Transaction[])account.Transactions;
        copy[0] = null!;

        Assert.Equal(10m, Assert.Single(account.Transactions).Amount);
    }
}
