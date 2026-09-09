using Atm.Api.Contracts;
using Atm.Application.Accounts;
using Microsoft.AspNetCore.Mvc;

namespace Atm.Api.Controllers;

/// <summary>
/// Read accounts and apply deposits and withdrawals. Actions are thin: they forward to an
/// application handler and let exceptions surface to the global handler.
/// </summary>
[ApiController]
[Route("accounts")]
public sealed class AccountsController(
    GetAccountsHandler getAccounts,
    DepositHandler deposit,
    WithdrawHandler withdraw) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AccountSnapshot>>> GetAll(
        CancellationToken cancellationToken)
    {
        return Ok(await getAccounts.Handle(cancellationToken));
    }

    [HttpPost("{id:guid}/deposit")]
    public async Task<ActionResult<AccountSnapshot>> Deposit(
        Guid id,
        AmountRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(await deposit.Handle(new DepositRequest(id, request.Amount), cancellationToken));
    }

    [HttpPost("{id:guid}/withdraw")]
    public async Task<ActionResult<AccountSnapshot>> Withdraw(
        Guid id,
        AmountRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(await withdraw.Handle(new WithdrawRequest(id, request.Amount), cancellationToken));
    }
}
