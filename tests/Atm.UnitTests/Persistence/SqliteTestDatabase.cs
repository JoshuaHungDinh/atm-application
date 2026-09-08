using Atm.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Atm.UnitTests.Persistence;

/// <summary>
/// One SQLite in-memory database, held open for the lifetime of a single test by a
/// kept-alive connection. <see cref="NewContext"/> hands out fresh <see cref="AtmDbContext"/>
/// instances over that database, so a test can write with one context and assert with
/// another — proving the round-trip really hit storage.
/// </summary>
internal sealed class SqliteTestDatabase : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<AtmDbContext> _options;

    public SqliteTestDatabase()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        _options = new DbContextOptionsBuilder<AtmDbContext>()
            .UseSqlite(_connection)
            .Options;

        using AtmDbContext context = NewContext();
        context.Database.Migrate();
    }

    public AtmDbContext NewContext() => new(_options);

    public void Dispose() => _connection.Dispose();
}
