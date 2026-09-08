using Atm.Domain.Accounts;

namespace Atm.Application.Abstractions;

/// <summary>
/// Persistence gateway for <see cref="Account"/> aggregates. The application layer depends
/// on this abstraction; the implementation lives in the infrastructure layer.
/// </summary>
public interface IAccountRepository
{
    Task<Account?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Account>> GetAllAsync(CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
