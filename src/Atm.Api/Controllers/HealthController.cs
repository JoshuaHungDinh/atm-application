using Microsoft.AspNetCore.Mvc;

namespace Atm.Api.Controllers;

/// <summary>
/// Liveness probe. Used by CI, the frontend's connectivity check, and manual smoke tests.
/// </summary>
[ApiController]
[Route("health")]
public sealed class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok(new { status = "ok" });
}
