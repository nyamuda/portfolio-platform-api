using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortfolioPlatform.Api.Dtos.Experiences;
using PortfolioPlatform.Api.Exceptions;
using PortfolioPlatform.Api.Models;
using PortfolioPlatform.Api.Services.Abstractions.Auth;
using PortfolioPlatform.Api.Services.Abstractions.Experiences;

namespace PortfolioPlatform.Api.Controllers;

/// <summary>
/// Handles timeline experience endpoints for profile owners and public visitors.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class ExperiencesController(
    IExperienceService experienceService,
    IJwtService jwtService,
    ILogger<ExperiencesController> logger
) : ControllerBase
{
    private readonly IExperienceService _experienceService = experienceService;
    private readonly IJwtService _jwtService = jwtService;
    private readonly ILogger<ExperiencesController> _logger = logger;

    /// <summary>
    /// Returns a paginated page of experiences owned by the authenticated user.
    /// </summary>
    /// <param name="filters">The filters and paging values supplied by the owner list page.</param>
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetMine([FromQuery] ExperienceFilters filters)
    {
        try
        {
            int userId = GetAuthenticatedUserId();
            PageInfo<ExperienceDto> experiences = await _experienceService.GetMineAsync(userId, filters);
            return Ok(experiences);
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
            _logger.LogError(exception, "Failed to load the authenticated user's experiences.");
            return StatusCode(500, ErrorResponse.Unexpected());
        }
    }

    /// <summary>
    /// Returns one experience owned by the authenticated user.
    /// </summary>
    /// <param name="experienceId">The experience identifier.</param>
    [HttpGet("me/{experienceId:int}")]
    [Authorize]
    public async Task<IActionResult> GetMineById(int experienceId)
    {
        try
        {
            int userId = GetAuthenticatedUserId();
            ExperienceDto experience = await _experienceService.GetMineByIdAsync(userId, experienceId);
            return Ok(experience);
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
            _logger.LogError(exception, "Failed to load experience {ExperienceId}.", experienceId);
            return StatusCode(500, ErrorResponse.Unexpected());
        }
    }

    /// <summary>
    /// Returns published timeline experiences for a published public profile.
    /// </summary>
    /// <param name="profileSlug">The public profile slug.</param>
    [HttpGet("profile/{profileSlug}")]
    public async Task<IActionResult> GetPublicForProfile(string profileSlug)
    {
        try
        {
            // Public timeline lists only include entries the profile owner has chosen to publish.
            List<ExperienceDto> experiences = await _experienceService.GetPublicByProfileSlugAsync(profileSlug);
            return Ok(experiences);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Failed to load public experiences for profile '{ProfileSlug}'.",
                profileSlug
            );
            return StatusCode(500, ErrorResponse.Unexpected());
        }
    }

    /// <summary>
    /// Creates an experience for the authenticated user's profile.
    /// </summary>
    /// <param name="dto">The experience details to create.</param>
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create(UpsertExperienceDto dto)
    {
        try
        {
            int userId = GetAuthenticatedUserId();
            ExperienceDto experience = await _experienceService.CreateAsync(userId, dto);
            return CreatedAtAction(nameof(GetMineById), new { experienceId = experience.Id }, experience);
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
            _logger.LogError(exception, "Failed to create an experience.");
            return StatusCode(500, ErrorResponse.Unexpected());
        }
    }

    /// <summary>
    /// Updates an experience owned by the authenticated user's profile.
    /// </summary>
    /// <param name="experienceId">The experience identifier.</param>
    /// <param name="dto">The experience details to save.</param>
    [HttpPut("{experienceId:int}")]
    [Authorize]
    public async Task<IActionResult> Update(int experienceId, UpsertExperienceDto dto)
    {
        try
        {
            int userId = GetAuthenticatedUserId();
            ExperienceDto experience = await _experienceService.UpdateAsync(userId, experienceId, dto);
            return Ok(experience);
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
            _logger.LogError(exception, "Failed to update experience {ExperienceId}.", experienceId);
            return StatusCode(500, ErrorResponse.Unexpected());
        }
    }

    /// <summary>
    /// Deletes an experience owned by the authenticated user's profile.
    /// </summary>
    /// <param name="experienceId">The experience identifier.</param>
    [HttpDelete("{experienceId:int}")]
    [Authorize]
    public async Task<IActionResult> Delete(int experienceId)
    {
        try
        {
            int userId = GetAuthenticatedUserId();
            await _experienceService.DeleteAsync(userId, experienceId);
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
            _logger.LogError(exception, "Failed to delete experience {ExperienceId}.", experienceId);
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
