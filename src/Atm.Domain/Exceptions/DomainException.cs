namespace Atm.Domain.Exceptions;

/// <summary>
/// Base type for every business-rule violation raised by the domain. Callers can catch
/// this to handle all of them uniformly, or a specific subclass for finer control.
/// </summary>
public abstract class DomainException : Exception
{
    protected DomainException(string message)
        : base(message)
    {
    }
}
