using Atm.Domain.Accounts;
using Microsoft.EntityFrameworkCore;

namespace Atm.Infrastructure.Persistence;

/// <summary>
/// Creates the two accounts the application manages, the first time it runs against an
/// empty database. Idempotent — does nothing once the accounts exist.
/// </summary>
public sealed class AccountSeeder(AtmDbContext db)
{
    private static readonly Guid CheckingId = new("11111111-1111-1111-1111-111111111111");
    private static readonly Guid SavingsId = new("22222222-2222-2222-2222-222222222222");

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (await db.Accounts.AnyAsync(cancellationToken))
        {
            return;
        }

        db.Accounts.AddRange(
            new Account(CheckingId, "Checking", openingBalance: 1000m),
            new Account(SavingsId, "Savings", openingBalance: 500m));

        await db.SaveChangesAsync(cancellationToken);
    }
}
