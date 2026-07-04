using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortfolioPlatform.Api.Dtos.Dashboard;
using PortfolioPlatform.Api.Models;
using PortfolioPlatform.Api.Services.Abstractions.Auth;
using PortfolioPlatform.Api.Services.Abstractions.Dashboard;

namespace PortfolioPlatform.Api.Controllers;

/// <summary>
/// Returns authenticated dashboard data for profile setup and content progress.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class DashboardController(
    IDashboardService dashboardService,
    IJwtService jwtService,
    ILogger<DashboardController> logger
) : ControllerBase
{
    private readonly IDashboardService _dashboardService = dashboardService;
    private readonly IJwtService _jwtService = jwtService;
    private readonly ILogger<DashboardController> _logger = logger;

    /// <summary>
    /// Returns the authenticated user's dashboard summary.
    /// </summary>
    [HttpGet("summary")]
    [Authorize]
    public async Task<IActionResult> GetSummary()
    {
        try
        {
            int userId = GetAuthenticatedUserId();

            // The dashboard endpoint stays small: the service decides what guidance is useful.
            DashboardSummaryDto summary = await _dashboardService.GetSummaryAsync(userId);
            return Ok(summary);
        }
        catch (UnauthorizedAccessException exception)
        {
            return Unauthorized(ErrorResponse.Create(exception.Message));
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to load the dashboard summary.");
            return StatusCode(500, ErrorResponse.Unexpected());
        }
    }

    /// <summary>
    /// Extracts the authenticated user id from the bearer token on the current request.
    /// </summary>
    /// <returns>The authenticated user's id.</returns>
    private int GetAuthenticatedUserId()
    {
        // Keep JWT parsing in the JWT service so all controllers use the same token rules.
        string token = HttpContext.Request.Headers.Authorization.ToString().Replace("Bearer ", "");
        return _jwtService.ValidateTokenAndExtractUser(token).Id;
    }
}


