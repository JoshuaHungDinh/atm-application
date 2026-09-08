using Atm.Application.Abstractions;
using Atm.Domain.Accounts;

namespace Atm.Application.Accounts;

/// <summary>Moves funds between two accounts and persists both in a single unit of work.</summary>
public sealed class TransferHandler(
    IAccountRepository accounts,
    MoneyTransferService transfers,
    TimeProvider clock)
{
    public async Task<TransferResult> Handle(
        TransferRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        Account from = await accounts.GetByIdAsync(request.FromAccountId, cancellationToken)
            ?? throw new AccountNotFoundException(request.FromAccountId);
        Account to = await accounts.GetByIdAsync(request.ToAccountId, cancellationToken)
            ?? throw new AccountNotFoundException(request.ToAccountId);

        transfers.Transfer(from, to, request.Amount, clock);
        await accounts.SaveChangesAsync(cancellationToken);

        return new TransferResult(AccountSnapshot.From(from), AccountSnapshot.From(to));
    }
}

/// <summary>Transfer <paramref name="Amount"/> from <paramref name="FromAccountId"/> to <paramref name="ToAccountId"/>.</summary>
public sealed record TransferRequest(Guid FromAccountId, Guid ToAccountId, decimal Amount);

/// <summary>The two accounts as they stand after a completed transfer.</summary>
public sealed record TransferResult(AccountSnapshot From, AccountSnapshot To);
