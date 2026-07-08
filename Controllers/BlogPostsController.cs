using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortfolioPlatform.Api.Dtos.BlogPosts;
using PortfolioPlatform.Api.Exceptions;
using PortfolioPlatform.Api.Models;
using PortfolioPlatform.Api.Services.Abstractions.Auth;
using PortfolioPlatform.Api.Services.Abstractions.BlogPosts;

namespace PortfolioPlatform.Api.Controllers;

/// <summary>
/// Handles blog post endpoints for profile owners and public visitors.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class BlogPostsController(
    IBlogPostService blogPostService,
    IJwtService jwtService,
    ILogger<BlogPostsController> logger
) : ControllerBase
{
    private readonly IBlogPostService _blogPostService = blogPostService;
    private readonly IJwtService _jwtService = jwtService;
    private readonly ILogger<BlogPostsController> _logger = logger;

    /// <summary>
    /// Returns a paginated page of blog posts owned by the authenticated user.
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetMine([FromQuery] BlogPostFilters filters)
    {
        try
        {
            int userId = GetAuthenticatedUserId();
            PageInfo<BlogPostDto> posts = await _blogPostService.GetMineAsync(userId, filters);
            return Ok(posts);
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
            _logger.LogError(exception, "Failed to load the authenticated user's blog posts.");
            return StatusCode(500, ErrorResponse.Unexpected());
        }
    }

    /// <summary>
    /// Returns one blog post owned by the authenticated user.
    /// </summary>
    /// <param name="postId">The blog post identifier.</param>
    [HttpGet("me/{postId:int}")]
    [Authorize]
    public async Task<IActionResult> GetMineById(int postId)
    {
        try
        {
            int userId = GetAuthenticatedUserId();
            BlogPostDto post = await _blogPostService.GetMineByIdAsync(userId, postId);
            return Ok(post);
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
            _logger.LogError(exception, "Failed to load blog post {PostId}.", postId);
            return StatusCode(500, ErrorResponse.Unexpected());
        }
    }

    /// <summary>
    /// Returns published blog posts for a published public profile.
    /// </summary>
    /// <param name="profileSlug">The public profile slug.</param>
    [HttpGet("profile/{profileSlug}")]
    public async Task<IActionResult> GetPublicForProfile(string profileSlug)
    {
        try
        {
            // Public post lists only include posts that the owner has chosen to publish.
            List<BlogPostDto> posts = await _blogPostService.GetPublicByProfileSlugAsync(profileSlug);
            return Ok(posts);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to load public blog posts for profile '{ProfileSlug}'.", profileSlug);
            return StatusCode(500, ErrorResponse.Unexpected());
        }
    }

    /// <summary>
    /// Returns a published blog post from a published public profile.
    /// </summary>
    /// <param name="profileSlug">The public profile slug.</param>
    /// <param name="postSlug">The public blog post slug.</param>
    [HttpGet("profile/{profileSlug}/{postSlug}")]
    public async Task<IActionResult> GetPublicBySlug(string profileSlug, string postSlug)
    {
        try
        {
            BlogPostDto post = await _blogPostService.GetPublicBySlugAsync(profileSlug, postSlug);
            return Ok(post);
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(ErrorResponse.Create(exception.Message));
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Failed to load public blog post '{PostSlug}' for profile '{ProfileSlug}'.",
                postSlug,
                profileSlug
            );
            return StatusCode(500, ErrorResponse.Unexpected());
        }
    }

    /// <summary>
    /// Creates a blog post for the authenticated user's profile.
    /// </summary>
    /// <param name="dto">The blog post details to create.</param>
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create(UpsertBlogPostDto dto)
    {
        try
        {
            int userId = GetAuthenticatedUserId();
            BlogPostDto post = await _blogPostService.CreateAsync(userId, dto);
            return CreatedAtAction(nameof(GetMineById), new { postId = post.Id }, post);
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
            _logger.LogError(exception, "Failed to create a blog post.");
            return StatusCode(500, ErrorResponse.Unexpected());
        }
    }

    /// <summary>
    /// Updates a blog post owned by the authenticated user's profile.
    /// </summary>
    /// <param name="postId">The blog post identifier.</param>
    /// <param name="dto">The blog post details to save.</param>
    [HttpPut("{postId:int}")]
    [Authorize]
    public async Task<IActionResult> Update(int postId, UpsertBlogPostDto dto)
    {
        try
        {
            int userId = GetAuthenticatedUserId();
            BlogPostDto post = await _blogPostService.UpdateAsync(userId, postId, dto);
            return Ok(post);
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
            _logger.LogError(exception, "Failed to update blog post {PostId}.", postId);
            return StatusCode(500, ErrorResponse.Unexpected());
        }
    }

    /// <summary>
    /// Deletes a blog post owned by the authenticated user's profile.
    /// </summary>
    /// <param name="postId">The blog post identifier.</param>
    [HttpDelete("{postId:int}")]
    [Authorize]
    public async Task<IActionResult> Delete(int postId)
    {
        try
        {
            int userId = GetAuthenticatedUserId();
            await _blogPostService.DeleteAsync(userId, postId);
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
            _logger.LogError(exception, "Failed to delete blog post {PostId}.", postId);
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




