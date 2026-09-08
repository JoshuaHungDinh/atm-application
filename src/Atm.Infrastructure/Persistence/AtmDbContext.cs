using Atm.Domain.Accounts;
using Microsoft.EntityFrameworkCore;

namespace Atm.Infrastructure.Persistence;

/// <summary>
/// EF Core context for the account aggregate. All persistence mapping lives here so the
/// domain and application layers stay free of ORM concerns.
/// </summary>
public sealed class AtmDbContext(DbContextOptions<AtmDbContext> options) : DbContext(options)
{
    public DbSet<Account> Accounts => Set<Account>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Account>(account =>
        {
            account.HasKey(a => a.Id);
            account.Property(a => a.Id).ValueGeneratedNever();
            account.Property(a => a.Name).IsRequired().HasMaxLength(100);
            account.Property(a => a.Balance).HasPrecision(19, 4);

            // History is persisted through the backing field; the public property is a
            // read-only snapshot, not a navigation.
            account.Ignore(a => a.Transactions);

            account.OwnsMany(typeof(Transaction), "_transactions", transaction =>
            {
                transaction.ToTable("Transactions");
                transaction.WithOwner().HasForeignKey("AccountId");
                transaction.HasKey("Id");
                transaction.Property("Id").ValueGeneratedNever();
                transaction.Property("Amount").HasPrecision(19, 4);
                transaction.Property("BalanceAfter").HasPrecision(19, 4);
                transaction.Property("Type").HasConversion<string>().HasMaxLength(16);
                transaction.Property("OccurredAt");
                transaction.Property("Description").HasMaxLength(200);
            });

            account.Navigation("_transactions").UsePropertyAccessMode(PropertyAccessMode.Field);
        });
    }
}
