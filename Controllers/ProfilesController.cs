using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortfolioPlatform.Api.Dtos.Profiles;
using PortfolioPlatform.Api.Exceptions;
using PortfolioPlatform.Api.Models;
using PortfolioPlatform.Api.Services.Abstractions.Auth;
using PortfolioPlatform.Api.Services.Abstractions.Profiles;

namespace PortfolioPlatform.Api.Controllers;

/// <summary>
/// Handles owner and public profile endpoints.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class ProfilesController(
    IProfileService profileService,
    IJwtService jwtService,
    ILogger<ProfilesController> logger
) : ControllerBase
{
    private readonly IProfileService _profileService = profileService;
    private readonly IJwtService _jwtService = jwtService;
    private readonly ILogger<ProfilesController> _logger = logger;

    /// <summary>
    /// Returns the authenticated user's profile, including drafts and unpublished data.
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetMine()
    {
        try
        {
            int userId = GetAuthenticatedUserId();
            ProfileDto? profile = await _profileService.GetMineAsync(userId);

            if (profile is null)
                return NotFound(ErrorResponse.Create("Profile was not found."));

            return Ok(profile);
        }
        catch (UnauthorizedAccessException exception)
        {
            return Unauthorized(ErrorResponse.Create(exception.Message));
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to load the authenticated user's profile.");
            return StatusCode(500, ErrorResponse.Unexpected());
        }
    }

    /// <summary>
    /// Returns a published profile by slug.
    /// </summary>
    /// <param name="slug">The public profile slug.</param>
    [HttpGet("{slug}")]
    public async Task<IActionResult> GetPublicBySlug(string slug)
    {
        try
        {
            // Public profile routes must never expose drafts or private profile data.
            ProfileDto profile = await _profileService.GetPublicBySlugAsync(slug);
            return Ok(profile);
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(ErrorResponse.Create(exception.Message));
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to load public profile '{Slug}'.", slug);
            return StatusCode(500, ErrorResponse.Unexpected());
        }
    }

    /// <summary>
    /// Creates or updates the authenticated user's profile.
    /// </summary>
    /// <param name="dto">The profile details to save.</param>
    [HttpPut("me")]
    [Authorize]
    public async Task<IActionResult> UpsertMine(UpsertProfileDto dto)
    {
        try
        {
            int userId = GetAuthenticatedUserId();

            // The service owns the create-or-update decision so the controller stays thin.
            ProfileDto profile = await _profileService.UpsertAsync(userId, dto);
            return Ok(profile);
        }
        catch (ConflictException exception)
        {
            return StatusCode(409, ErrorResponse.Create(exception.Message));
        }
        catch (UnauthorizedAccessException exception)
        {
            return Unauthorized(ErrorResponse.Create(exception.Message));
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to save the authenticated user's profile.");
            return StatusCode(500, ErrorResponse.Unexpected());
        }
    }

    /// <summary>
    /// Deletes the authenticated user's profile and the content attached to it.
    /// </summary>
    [HttpDelete("me")]
    [Authorize]
    public async Task<IActionResult> DeleteMine()
    {
        try
        {
            int userId = GetAuthenticatedUserId();
            await _profileService.DeleteMineAsync(userId);
            return NoContent();
        }
        catch (UnauthorizedAccessException exception)
        {
            return Unauthorized(ErrorResponse.Create(exception.Message));
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to delete the authenticated user's profile.");
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



