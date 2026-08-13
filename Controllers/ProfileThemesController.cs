using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortfolioPlatform.Api.Dtos.Themes;
using PortfolioPlatform.Api.Models;
using PortfolioPlatform.Api.Services.Abstractions.Auth;
using PortfolioPlatform.Api.Services.Abstractions.Themes;

namespace PortfolioPlatform.Api.Controllers;

/// <summary>
/// Reads and updates the presentation choices used by public portfolio pages.
/// </summary>
[Route("api/profiles")]
[ApiController]
public class ProfileThemesController(
    IPortfolioThemeService themeService,
    IJwtService jwtService,
    ILogger<ProfileThemesController> logger
) : ControllerBase
{
    private readonly IPortfolioThemeService _themeService = themeService;
    private readonly IJwtService _jwtService = jwtService;
    private readonly ILogger<ProfileThemesController> _logger = logger;

    /// <summary>Returns the authenticated owner's current portfolio design.</summary>
    [HttpGet("me/theme")]
    [Authorize]
    public async Task<IActionResult> GetMine()
    {
        try { return Ok(await _themeService.GetMineAsync(GetAuthenticatedUserId())); }
        catch (KeyNotFoundException exception) { return NotFound(ErrorResponse.Create(exception.Message)); }
        catch (UnauthorizedAccessException exception) { return Unauthorized(ErrorResponse.Create(exception.Message)); }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to load the owner's portfolio theme.");
            return StatusCode(500, ErrorResponse.Unexpected());
        }
    }

    /// <summary>Updates the authenticated owner's theme, accent colour, and typography.</summary>
    [HttpPut("me/theme")]
    [Authorize]
    public async Task<IActionResult> UpdateMine(UpdateThemeSelectionDto dto)
    {
        try { return Ok(await _themeService.UpdateMineAsync(GetAuthenticatedUserId(), dto)); }
        catch (KeyNotFoundException exception) { return NotFound(ErrorResponse.Create(exception.Message)); }
        catch (UnauthorizedAccessException exception) { return Unauthorized(ErrorResponse.Create(exception.Message)); }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to update the owner's portfolio theme.");
            return StatusCode(500, ErrorResponse.Unexpected());
        }
    }

    /// <summary>Returns the design used by one published public profile.</summary>
    [HttpGet("{profileSlug}/theme")]
    public async Task<IActionResult> GetPublicTheme(string profileSlug)
    {
        try { return Ok(await _themeService.GetPublicThemeAsync(profileSlug)); }
        catch (KeyNotFoundException exception) { return NotFound(ErrorResponse.Create(exception.Message)); }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to resolve the public theme for profile '{ProfileSlug}'.", profileSlug);
            return StatusCode(500, ErrorResponse.Unexpected());
        }
    }

    /// <summary>Extracts the authenticated user id using the shared JWT rules.</summary>
    private int GetAuthenticatedUserId()
    {
        string token = HttpContext.Request.Headers.Authorization.ToString().Replace("Bearer ", "");
        return _jwtService.ValidateTokenAndExtractUser(token).Id;
    }
}
