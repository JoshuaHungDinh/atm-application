using Atm.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Atm.UnitTests.Api;

/// <summary>
/// Boots the real API against a private in-memory SQLite database, kept alive by a single
/// open connection. Startup still migrates and seeds, so every instance begins with the
/// two seeded accounts.
/// </summary>
internal sealed class AtmApiFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection = new("Data Source=:memory:");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        _connection.Open();

        builder.ConfigureServices(services =>
        {
            // Drop the app's file-backed SQLite registration and point the context at the
            // in-memory connection this factory keeps open.
            services.RemoveAll<DbContextOptions<AtmDbContext>>();
            services.RemoveAll<AtmDbContext>();

            services.AddDbContext<AtmDbContext>(options => options.UseSqlite(_connection));
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
        {
            _connection.Dispose();
        }
    }
}
