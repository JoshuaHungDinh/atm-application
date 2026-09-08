using System.Globalization;

namespace Atm.Domain.Exceptions;

/// <summary>
/// Raised when a withdrawal would take an account below zero. Carries the balance and the
/// requested amount so callers can explain the failure without re-deriving it.
/// </summary>
public sealed class InsufficientFundsException : DomainException
{
    public InsufficientFundsException(decimal balance, decimal requested)
        : base(
            string.Format(
                CultureInfo.InvariantCulture,
                "Cannot withdraw {0:0.00}: balance is {1:0.00}.",
                requested,
                balance))
    {
        Balance = balance;
        Requested = requested;
    }

    public decimal Balance { get; }

    public decimal Requested { get; }
}
