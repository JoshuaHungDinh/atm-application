namespace Atm.Api.Contracts;

/// <summary>Request body for a deposit or withdrawal.</summary>
public sealed record AmountRequest(decimal Amount);
