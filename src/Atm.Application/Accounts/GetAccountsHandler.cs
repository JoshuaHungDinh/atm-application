using Atm.Application.Abstractions;
using Atm.Domain.Accounts;

namespace Atm.Application.Accounts;

/// <summary>Returns every account with its transaction history.</summary>
public sealed class GetAccountsHandler(IAccountRepository accounts)
{
    public async Task<IReadOnlyList<AccountSnapshot>> Handle(
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Account> all = await accounts.GetAllAsync(cancellationToken);
        return all.Select(AccountSnapshot.From).ToArray();
    }
}
