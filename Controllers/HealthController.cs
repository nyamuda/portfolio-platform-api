using Microsoft.AspNetCore.Mvc;

namespace PortfolioPlatform.Api.Controllers;

/// <summary>
/// Lightweight health endpoint for smoke checks and deployment probes.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public ActionResult<object> Get()
    {
        return Ok(new
        {
            status = "ok",
            service = "portfolio-platform-api",
            timestampUtc = DateTime.UtcNow
        });
    }
}



