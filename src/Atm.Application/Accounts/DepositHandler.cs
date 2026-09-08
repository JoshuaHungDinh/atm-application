using Atm.Application.Abstractions;
using Atm.Domain.Accounts;

namespace Atm.Application.Accounts;

/// <summary>Applies a deposit to an account and persists it.</summary>
public sealed class DepositHandler(IAccountRepository accounts, TimeProvider clock)
{
    public async Task<AccountSnapshot> Handle(
        DepositRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        Account account = await accounts.GetByIdAsync(request.AccountId, cancellationToken)
            ?? throw new AccountNotFoundException(request.AccountId);

        account.Deposit(request.Amount, clock);
        await accounts.SaveChangesAsync(cancellationToken);

        return AccountSnapshot.From(account);
    }
}

/// <summary>Deposit <paramref name="Amount"/> into the account identified by <paramref name="AccountId"/>.</summary>
public sealed record DepositRequest(Guid AccountId, decimal Amount);
