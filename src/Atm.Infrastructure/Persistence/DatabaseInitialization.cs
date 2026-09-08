using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Atm.Infrastructure.Persistence;

/// <summary>Startup helper: apply migrations, then seed. Keeps EF Core out of the API host.</summary>
public static class DatabaseInitialization
{
    public static async Task InitializeAtmDatabaseAsync(
        this IServiceProvider services,
        CancellationToken cancellationToken = default)
    {
        await using AsyncServiceScope scope = services.CreateAsyncScope();

        AtmDbContext db = scope.ServiceProvider.GetRequiredService<AtmDbContext>();
        await db.Database.MigrateAsync(cancellationToken);

        AccountSeeder seeder = scope.ServiceProvider.GetRequiredService<AccountSeeder>();
        await seeder.SeedAsync(cancellationToken);
    }
}
