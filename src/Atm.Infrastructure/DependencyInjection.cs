using Atm.Application.Abstractions;
using Atm.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Atm.Infrastructure;

/// <summary>
/// Composition root for the infrastructure layer (persistence and other external
/// concerns). Keeps EF Core / SQLite wiring out of the API's startup code.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        string connectionString = configuration.GetConnectionString("AtmDb")
            ?? throw new InvalidOperationException("Connection string 'AtmDb' is not configured.");

        services.AddDbContext<AtmDbContext>(options => options.UseSqlite(connectionString));
        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<AccountSeeder>();

        return services;
    }
}
