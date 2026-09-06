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
        // In PR 3 this registers AtmDbContext against
        // configuration.GetConnectionString("AtmDb") plus the repository.
        _ = configuration;
        return services;
    }
}
