namespace Atm.Api.Contracts;

/// <summary>
/// Request body for <c>POST /transfers</c>. Named with a <c>Body</c> suffix to avoid
/// colliding with the application layer's <c>TransferRequest</c>.
/// </summary>
public sealed record TransferRequestBody(Guid FromAccountId, Guid ToAccountId, decimal Amount);
