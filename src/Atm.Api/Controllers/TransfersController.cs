using Atm.Api.Contracts;
using Atm.Application.Accounts;
using Microsoft.AspNetCore.Mvc;

namespace Atm.Api.Controllers;

/// <summary>Move funds between two accounts.</summary>
[ApiController]
[Route("transfers")]
public sealed class TransfersController(TransferHandler transfers) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<TransferResult>> Transfer(
        TransferRequestBody request,
        CancellationToken cancellationToken)
    {
        TransferResult result = await transfers.Handle(
            new TransferRequest(request.FromAccountId, request.ToAccountId, request.Amount),
            cancellationToken);

        return Ok(result);
    }
}
