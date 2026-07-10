using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortfolioPlatform.Api.Dtos.Offerings;
using PortfolioPlatform.Api.Exceptions;
using PortfolioPlatform.Api.Models;
using PortfolioPlatform.Api.Services.Abstractions.Auth;
using PortfolioPlatform.Api.Services.Abstractions.Offerings;

namespace PortfolioPlatform.Api.Controllers;

/// <summary>
/// Handles offering endpoints for profile owners and public visitors.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class OfferingsController(
    IOfferingService offeringService,
    IJwtService jwtService,
    ILogger<OfferingsController> logger
) : ControllerBase
{
    private readonly IOfferingService _offeringService = offeringService;
    private readonly IJwtService _jwtService = jwtService;
    private readonly ILogger<OfferingsController> _logger = logger;

    /// <summary>
    /// Returns a paginated page of offerings owned by the authenticated user.
    /// </summary>
    /// <param name="filters">The filters and paging values supplied by the owner list page.</param>
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetMine([FromQuery] OfferingFilters filters)
    {
        try
        {
            int userId = GetAuthenticatedUserId();
            PageInfo<OfferingDto> offerings = await _offeringService.GetMineAsync(userId, filters);
            return Ok(offerings);
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(ErrorResponse.Create(exception.Message));
        }
        catch (UnauthorizedAccessException exception)
        {
            return Unauthorized(ErrorResponse.Create(exception.Message));
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to load the authenticated user's offerings.");
            return StatusCode(500, ErrorResponse.Unexpected());
        }
    }

    /// <summary>
    /// Returns one offering owned by the authenticated user.
    /// </summary>
    /// <param name="offeringId">The offering identifier.</param>
    [HttpGet("me/{offeringId:int}")]
    [Authorize]
    public async Task<IActionResult> GetMineById(int offeringId)
    {
        try
        {
            int userId = GetAuthenticatedUserId();
            OfferingDto offering = await _offeringService.GetMineByIdAsync(userId, offeringId);
            return Ok(offering);
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(ErrorResponse.Create(exception.Message));
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(ErrorResponse.Create(exception.Message));
        }
        catch (UnauthorizedAccessException exception)
        {
            return Unauthorized(ErrorResponse.Create(exception.Message));
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to load offering {OfferingId}.", offeringId);
            return StatusCode(500, ErrorResponse.Unexpected());
        }
    }

    /// <summary>
    /// Returns published offerings for a published public profile.
    /// </summary>
    /// <param name="profileSlug">The public profile slug.</param>
    [HttpGet("profile/{profileSlug}")]
    public async Task<IActionResult> GetPublicForProfile(string profileSlug)
    {
        try
        {
            // Public offering lists only include work the profile owner has chosen to publish.
            List<OfferingDto> offerings = await _offeringService.GetPublicByProfileSlugAsync(profileSlug);
            return Ok(offerings);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Failed to load public offerings for profile '{ProfileSlug}'.",
                profileSlug
            );
            return StatusCode(500, ErrorResponse.Unexpected());
        }
    }

    /// <summary>
    /// Returns a published offering from a published public profile.
    /// </summary>
    /// <param name="profileSlug">The public profile slug.</param>
    /// <param name="offeringSlug">The public offering slug.</param>
    [HttpGet("profile/{profileSlug}/{offeringSlug}")]
    public async Task<IActionResult> GetPublicBySlug(string profileSlug, string offeringSlug)
    {
        try
        {
            OfferingDto offering = await _offeringService.GetPublicBySlugAsync(profileSlug, offeringSlug);
            return Ok(offering);
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(ErrorResponse.Create(exception.Message));
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Failed to load public offering '{OfferingSlug}' for profile '{ProfileSlug}'.",
                offeringSlug,
                profileSlug
            );
            return StatusCode(500, ErrorResponse.Unexpected());
        }
    }

    /// <summary>
    /// Creates an offering for the authenticated user's profile.
    /// </summary>
    /// <param name="dto">The offering details to create.</param>
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create(UpsertOfferingDto dto)
    {
        try
        {
            int userId = GetAuthenticatedUserId();
            OfferingDto offering = await _offeringService.CreateAsync(userId, dto);
            return CreatedAtAction(nameof(GetMineById), new { offeringId = offering.Id }, offering);
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(ErrorResponse.Create(exception.Message));
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
            _logger.LogError(exception, "Failed to create an offering.");
            return StatusCode(500, ErrorResponse.Unexpected());
        }
    }

    /// <summary>
    /// Updates an offering owned by the authenticated user's profile.
    /// </summary>
    /// <param name="offeringId">The offering identifier.</param>
    /// <param name="dto">The offering details to save.</param>
    [HttpPut("{offeringId:int}")]
    [Authorize]
    public async Task<IActionResult> Update(int offeringId, UpsertOfferingDto dto)
    {
        try
        {
            int userId = GetAuthenticatedUserId();
            OfferingDto offering = await _offeringService.UpdateAsync(userId, offeringId, dto);
            return Ok(offering);
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(ErrorResponse.Create(exception.Message));
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(ErrorResponse.Create(exception.Message));
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
            _logger.LogError(exception, "Failed to update offering {OfferingId}.", offeringId);
            return StatusCode(500, ErrorResponse.Unexpected());
        }
    }

    /// <summary>
    /// Deletes an offering owned by the authenticated user's profile.
    /// </summary>
    /// <param name="offeringId">The offering identifier.</param>
    [HttpDelete("{offeringId:int}")]
    [Authorize]
    public async Task<IActionResult> Delete(int offeringId)
    {
        try
        {
            int userId = GetAuthenticatedUserId();
            await _offeringService.DeleteAsync(userId, offeringId);
            return NoContent();
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(ErrorResponse.Create(exception.Message));
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(ErrorResponse.Create(exception.Message));
        }
        catch (UnauthorizedAccessException exception)
        {
            return Unauthorized(ErrorResponse.Create(exception.Message));
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to delete offering {OfferingId}.", offeringId);
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
