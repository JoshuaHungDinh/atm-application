using Atm.Application.Accounts;
using Atm.Domain.Accounts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Atm.Application;

/// <summary>
/// Composition root for the application layer. The API calls this so it never has to
/// know which concrete use-case handlers exist.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.TryAddSingleton(TimeProvider.System);
        services.AddSingleton<MoneyTransferService>();

        services.AddScoped<DepositHandler>();
        services.AddScoped<WithdrawHandler>();
        services.AddScoped<TransferHandler>();
        services.AddScoped<GetAccountsHandler>();

        return services;
    }
}
