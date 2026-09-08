using Atm.Domain.Accounts;
using Atm.Domain.Exceptions;
using Atm.UnitTests.TestDoubles;

namespace Atm.UnitTests.Accounts;

/// <summary>Tests for the cross-aggregate rules in <see cref="MoneyTransferService"/>.</summary>
public class MoneyTransferServiceTests
{
    private static readonly DateTimeOffset At = new(2026, 9, 7, 12, 0, 0, TimeSpan.Zero);
    private readonly FixedClock _clock = new(At);
    private readonly MoneyTransferService _service = new();

    private static Account NewAccount(string name, decimal openingBalance) =>
        new(Guid.NewGuid(), name, openingBalance);

    [Fact]
    public void Transfer_moves_the_amount_and_labels_both_legs()
    {
        Account from = NewAccount("Checking", 200m);
        Account to = NewAccount("Savings", 50m);

        _service.Transfer(from, to, 75m, _clock);

        Assert.Equal(125m, from.Balance);
        Assert.Equal(125m, to.Balance);
        Assert.Equal("Transfer to Savings", Assert.Single(from.Transactions).Description);
        Assert.Equal("Transfer from Checking", Assert.Single(to.Transactions).Description);
    }

    [Fact]
    public void Transfer_to_the_same_account_is_rejected()
    {
        Account account = NewAccount("Checking", 200m);

        Assert.Throws<InvalidTransferException>(
            () => _service.Transfer(account, account, 10m, _clock));
        Assert.Equal(200m, account.Balance);
        Assert.Empty(account.Transactions);
    }

    [Fact]
    public void Transfer_that_overdraws_the_source_is_rejected_before_either_leg_is_applied()
    {
        Account from = NewAccount("Checking", 40m);
        Account to = NewAccount("Savings", 0m);

        Assert.Throws<InsufficientFundsException>(
            () => _service.Transfer(from, to, 100m, _clock));
        Assert.Equal(40m, from.Balance);
        Assert.Equal(0m, to.Balance);
        Assert.Empty(from.Transactions);
        Assert.Empty(to.Transactions);
    }
}
