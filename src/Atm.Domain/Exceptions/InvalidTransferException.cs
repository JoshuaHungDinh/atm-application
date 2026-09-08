namespace Atm.Domain.Exceptions;

/// <summary>
/// Raised when a transfer is not permitted — for example, moving funds from an account to
/// itself.
/// </summary>
public sealed class InvalidTransferException : DomainException
{
    public InvalidTransferException(string message)
        : base(message)
    {
    }
}
