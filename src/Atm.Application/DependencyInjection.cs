using Microsoft.Extensions.DependencyInjection;

namespace Atm.Application;

/// <summary>
/// Composition root for the application layer. The API calls this so it never has to
/// know which concrete use-case services exist.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Use-case services (deposit / withdraw / transfer) are registered here in PR 3.
        return services;
    }
}
