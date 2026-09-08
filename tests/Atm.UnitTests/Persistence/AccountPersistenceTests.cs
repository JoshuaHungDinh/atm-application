using Atm.Application.Accounts;
using Atm.Domain.Accounts;
using Atm.Domain.Exceptions;
using Atm.Infrastructure.Persistence;
using Atm.UnitTests.TestDoubles;

namespace Atm.UnitTests.Persistence;

/// <summary>
/// End-to-end tests for the handlers over a real <see cref="AtmDbContext"/> on SQLite:
/// every assertion reads through a fresh context to prove the change was persisted.
/// </summary>
public sealed class AccountPersistenceTests : IDisposable
{
    private static readonly DateTimeOffset At = new(2026, 9, 7, 12, 0, 0, TimeSpan.Zero);
    private readonly FixedClock _clock = new(At);
    private readonly SqliteTestDatabase _db = new();

    public void Dispose() => _db.Dispose();

    private async Task<Guid> SeedAccountAsync(string name, decimal openingBalance)
    {
        Guid id = Guid.NewGuid();
        await using AtmDbContext context = _db.NewContext();
        context.Accounts.Add(new Account(id, name, openingBalance));
        await context.SaveChangesAsync();
        return id;
    }

    private async Task<Account> ReloadAsync(Guid id)
    {
        await using AtmDbContext context = _db.NewContext();
        return await new AccountRepository(context).GetByIdAsync(id)
            ?? throw new InvalidOperationException($"Account {id} was not persisted.");
    }

    [Fact]
    public async Task Deposit_is_persisted_and_reloads_with_a_transaction()
    {
        Guid accountId = await SeedAccountAsync("Checking", 100m);

        await using (AtmDbContext context = _db.NewContext())
        {
            DepositHandler handler = new(new AccountRepository(context), _clock);
            await handler.Handle(new DepositRequest(accountId, 40m));
        }

        Account reloaded = await ReloadAsync(accountId);
        Assert.Equal(140m, reloaded.Balance);
        Transaction entry = Assert.Single(reloaded.Transactions);
        Assert.Equal(TransactionType.Deposit, entry.Type);
        Assert.Equal(40m, entry.Amount);
        Assert.Equal(140m, entry.BalanceAfter);
        Assert.Equal(At, entry.OccurredAt);
    }

    [Fact]
    public async Task Withdraw_is_persisted_and_reloads_with_a_transaction()
    {
        Guid accountId = await SeedAccountAsync("Checking", 100m);

        await using (AtmDbContext context = _db.NewContext())
        {
            WithdrawHandler handler = new(new AccountRepository(context), _clock);
            await handler.Handle(new WithdrawRequest(accountId, 30m));
        }

        Account reloaded = await ReloadAsync(accountId);
        Assert.Equal(70m, reloaded.Balance);
        Transaction entry = Assert.Single(reloaded.Transactions);
        Assert.Equal(TransactionType.Withdrawal, entry.Type);
        Assert.Equal(70m, entry.BalanceAfter);
    }

    [Fact]
    public async Task Transfer_moves_funds_and_records_a_labelled_entry_on_each_account()
    {
        Guid fromId = await SeedAccountAsync("Checking", 200m);
        Guid toId = await SeedAccountAsync("Savings", 50m);

        await using (AtmDbContext context = _db.NewContext())
        {
            TransferHandler handler = new(
                new AccountRepository(context), new MoneyTransferService(), _clock);
            await handler.Handle(new TransferRequest(fromId, toId, 75m));
        }

        Account from = await ReloadAsync(fromId);
        Account to = await ReloadAsync(toId);
        Assert.Equal(125m, from.Balance);
        Assert.Equal(125m, to.Balance);
        Assert.Equal("Transfer to Savings", Assert.Single(from.Transactions).Description);
        Assert.Equal("Transfer from Checking", Assert.Single(to.Transactions).Description);
    }

    [Fact]
    public async Task A_transfer_that_overdraws_is_rejected_and_persists_nothing()
    {
        Guid fromId = await SeedAccountAsync("Checking", 40m);
        Guid toId = await SeedAccountAsync("Savings", 0m);

        await using (AtmDbContext context = _db.NewContext())
        {
            TransferHandler handler = new(
                new AccountRepository(context), new MoneyTransferService(), _clock);
            await Assert.ThrowsAsync<InsufficientFundsException>(
                () => handler.Handle(new TransferRequest(fromId, toId, 100m)));
        }

        Account from = await ReloadAsync(fromId);
        Account to = await ReloadAsync(toId);
        Assert.Equal(40m, from.Balance);
        Assert.Equal(0m, to.Balance);
        Assert.Empty(from.Transactions);
        Assert.Empty(to.Transactions);
    }

    [Fact]
    public async Task GetAccounts_returns_every_account_with_history_oldest_first()
    {
        Guid checkingId = await SeedAccountAsync("Checking", 100m);
        await SeedAccountAsync("Savings", 500m);

        await using (AtmDbContext context = _db.NewContext())
        {
            AccountRepository repository = new(context);
            await new DepositHandler(repository, _clock).Handle(new DepositRequest(checkingId, 50m));
            await new WithdrawHandler(repository, _clock).Handle(new WithdrawRequest(checkingId, 20m));
        }

        await using AtmDbContext verify = _db.NewContext();
        IReadOnlyList<AccountSnapshot> accounts =
            await new GetAccountsHandler(new AccountRepository(verify)).Handle();

        Assert.Equal(2, accounts.Count);
        AccountSnapshot checking = accounts.Single(a => a.Id == checkingId);
        Assert.Equal(130m, checking.Balance);
        Assert.Equal(
            [TransactionType.Deposit, TransactionType.Withdrawal],
            checking.Transactions.Select(t => t.Type));
        Assert.Equal([150m, 130m], checking.Transactions.Select(t => t.BalanceAfter));
    }

    [Fact]
    public async Task Depositing_into_an_unknown_account_throws()
    {
        await using AtmDbContext context = _db.NewContext();
        DepositHandler handler = new(new AccountRepository(context), _clock);

        await Assert.ThrowsAsync<AccountNotFoundException>(
            () => handler.Handle(new DepositRequest(Guid.NewGuid(), 10m)));
    }
}
