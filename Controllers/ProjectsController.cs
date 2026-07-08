using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortfolioPlatform.Api.Dtos.Projects;
using PortfolioPlatform.Api.Exceptions;
using PortfolioPlatform.Api.Models;
using PortfolioPlatform.Api.Services.Abstractions.Auth;
using PortfolioPlatform.Api.Services.Abstractions.Projects;

namespace PortfolioPlatform.Api.Controllers;

/// <summary>
/// Handles project endpoints for profile owners and public visitors.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class ProjectsController(
    IProjectService projectService,
    IJwtService jwtService,
    ILogger<ProjectsController> logger
) : ControllerBase
{
    private readonly IProjectService _projectService = projectService;
    private readonly IJwtService _jwtService = jwtService;
    private readonly ILogger<ProjectsController> _logger = logger;

    /// <summary>
    /// Returns a paginated page of projects owned by the authenticated user.
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetMine([FromQuery] ProjectFilters filters)
    {
        try
        {
            int userId = GetAuthenticatedUserId();
            PageInfo<ProjectDto> projects = await _projectService.GetMineAsync(userId, filters);
            return Ok(projects);
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
            _logger.LogError(exception, "Failed to load the authenticated user's projects.");
            return StatusCode(500, ErrorResponse.Unexpected());
        }
    }

    /// <summary>
    /// Returns one project owned by the authenticated user.
    /// </summary>
    /// <param name="projectId">The project identifier.</param>
    [HttpGet("me/{projectId:int}")]
    [Authorize]
    public async Task<IActionResult> GetMineById(int projectId)
    {
        try
        {
            int userId = GetAuthenticatedUserId();
            ProjectDto project = await _projectService.GetMineByIdAsync(userId, projectId);
            return Ok(project);
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
            _logger.LogError(exception, "Failed to load project {ProjectId}.", projectId);
            return StatusCode(500, ErrorResponse.Unexpected());
        }
    }

    /// <summary>
    /// Returns published projects for a published public profile.
    /// </summary>
    /// <param name="profileSlug">The public profile slug.</param>
    [HttpGet("profile/{profileSlug}")]
    public async Task<IActionResult> GetPublicForProfile(string profileSlug)
    {
        try
        {
            // Public project lists only include work that the owner has chosen to publish.
            List<ProjectDto> projects = await _projectService.GetPublicByProfileSlugAsync(
                profileSlug
            );
            return Ok(projects);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Failed to load public projects for profile '{ProfileSlug}'.",
                profileSlug
            );
            return StatusCode(500, ErrorResponse.Unexpected());
        }
    }

    /// <summary>
    /// Returns a published project from a published public profile.
    /// </summary>
    /// <param name="profileSlug">The public profile slug.</param>
    /// <param name="projectSlug">The public project slug.</param>
    [HttpGet("profile/{profileSlug}/{projectSlug}")]
    public async Task<IActionResult> GetPublicBySlug(string profileSlug, string projectSlug)
    {
        try
        {
            ProjectDto project = await _projectService.GetPublicBySlugAsync(
                profileSlug,
                projectSlug
            );
            return Ok(project);
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(ErrorResponse.Create(exception.Message));
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Failed to load public project '{ProjectSlug}' for profile '{ProfileSlug}'.",
                projectSlug,
                profileSlug
            );
            return StatusCode(500, ErrorResponse.Unexpected());
        }
    }

    /// <summary>
    /// Creates a project for the authenticated user's profile.
    /// </summary>
    /// <param name="dto">The project details to create.</param>
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create(UpsertProjectDto dto)
    {
        try
        {
            int userId = GetAuthenticatedUserId();
            ProjectDto project = await _projectService.CreateAsync(userId, dto);
            return CreatedAtAction(nameof(GetMineById), new { projectId = project.Id }, project);
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
            _logger.LogError(exception, "Failed to create a project.");
            return StatusCode(500, ErrorResponse.Unexpected());
        }
    }

    /// <summary>
    /// Updates a project owned by the authenticated user's profile.
    /// </summary>
    /// <param name="projectId">The project identifier.</param>
    /// <param name="dto">The project details to save.</param>
    [HttpPut("{projectId:int}")]
    [Authorize]
    public async Task<IActionResult> Update(int projectId, UpsertProjectDto dto)
    {
        try
        {
            int userId = GetAuthenticatedUserId();
            ProjectDto project = await _projectService.UpdateAsync(userId, projectId, dto);
            return Ok(project);
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
            _logger.LogError(exception, "Failed to update project {ProjectId}.", projectId);
            return StatusCode(500, ErrorResponse.Unexpected());
        }
    }

    /// <summary>
    /// Deletes a project owned by the authenticated user's profile.
    /// </summary>
    /// <param name="projectId">The project identifier.</param>
    [HttpDelete("{projectId:int}")]
    [Authorize]
    public async Task<IActionResult> Delete(int projectId)
    {
        try
        {
            int userId = GetAuthenticatedUserId();
            await _projectService.DeleteAsync(userId, projectId);
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
            _logger.LogError(exception, "Failed to delete project {ProjectId}.", projectId);
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




