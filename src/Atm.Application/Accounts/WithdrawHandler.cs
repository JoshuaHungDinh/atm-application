using Atm.Application.Abstractions;
using Atm.Domain.Accounts;

namespace Atm.Application.Accounts;

/// <summary>Applies a withdrawal to an account and persists it.</summary>
public sealed class WithdrawHandler(IAccountRepository accounts, TimeProvider clock)
{
    public async Task<AccountSnapshot> Handle(
        WithdrawRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        Account account = await accounts.GetByIdAsync(request.AccountId, cancellationToken)
            ?? throw new AccountNotFoundException(request.AccountId);

        account.Withdraw(request.Amount, clock);
        await accounts.SaveChangesAsync(cancellationToken);

        return AccountSnapshot.From(account);
    }
}

/// <summary>Withdraw <paramref name="Amount"/> from the account identified by <paramref name="AccountId"/>.</summary>
public sealed record WithdrawRequest(Guid AccountId, decimal Amount);
