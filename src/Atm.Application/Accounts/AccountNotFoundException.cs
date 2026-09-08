namespace Atm.Application.Accounts;

/// <summary>
/// Thrown by a handler when no account exists for the requested id. This is an
/// application-level concern — the domain has no notion of a missing aggregate.
/// </summary>
public sealed class AccountNotFoundException(Guid accountId)
    : Exception($"No account was found with id '{accountId}'.")
{
    public Guid AccountId { get; } = accountId;
}
