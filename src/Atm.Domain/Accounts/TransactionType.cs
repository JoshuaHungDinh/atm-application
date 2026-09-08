namespace Atm.Domain.Accounts;

/// <summary>
/// The kind of balance movement a transaction records. The amount is always positive;
/// the type gives it direction.
/// </summary>
public enum TransactionType
{
    Deposit,
    Withdrawal,
}
