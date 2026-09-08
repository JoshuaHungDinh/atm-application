using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Atm.Infrastructure.Persistence;

/// <summary>
/// Lets the EF Core command-line tools build an <see cref="AtmDbContext"/> without starting
/// the API. Used only at design time — for example, <c>dotnet ef migrations add</c>.
/// </summary>
public sealed class AtmDbContextFactory : IDesignTimeDbContextFactory<AtmDbContext>
{
    public AtmDbContext CreateDbContext(string[] args)
    {
        DbContextOptions<AtmDbContext> options = new DbContextOptionsBuilder<AtmDbContext>()
            .UseSqlite("Data Source=atm.db")
            .Options;

        return new AtmDbContext(options);
    }
}
