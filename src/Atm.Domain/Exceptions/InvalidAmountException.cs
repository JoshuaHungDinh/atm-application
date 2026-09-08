namespace Atm.Domain.Exceptions;

/// <summary>
/// Raised when an amount is not well-formed money: it must be greater than zero (or zero,
/// for an opening balance) with at most two decimal places.
/// </summary>
public sealed class InvalidAmountException : DomainException
{
    private InvalidAmountException(string message)
        : base(message)
    {
    }

    public static InvalidAmountException MustBePositive() =>
        new("Amount must be greater than zero.");

    public static InvalidAmountException MustNotBeNegative() =>
        new("Amount must not be negative.");

    public static InvalidAmountException TooManyDecimalPlaces() =>
        new("Amount cannot have more than two decimal places.");
}
