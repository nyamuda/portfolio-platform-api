using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortfolioPlatform.Api.Dtos.Tags;
using PortfolioPlatform.Api.Enums.Tags;
using PortfolioPlatform.Api.Exceptions;
using PortfolioPlatform.Api.Models;
using PortfolioPlatform.Api.Models.Content;
using PortfolioPlatform.Api.Services.Abstractions.Tags;

namespace PortfolioPlatform.Api.Controllers;

/// <summary>
/// Handles tag lookup for public screens and tag management for administrators.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class TagsController(ITagService tagService, ILogger<TagsController> logger) : ControllerBase
{
    private readonly ITagService _tagService = tagService;
    private readonly ILogger<TagsController> _logger = logger;

    /// <summary>
    /// Gets a paginated list of tags used across public portfolio content.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Get(
        string? search = null,
        int page = 1,
        int pageSize = 10,
        TagSortOption sortBy = TagSortOption.Popularity
    )
    {
        try
        {
            TagQueryParams queryParams = new()
            {
                Page = page,
                PageSize = pageSize,
                SortBy = sortBy,
                Search = search
            };

            PageInfo<TagDto> tags = await _tagService.GetTagsAsync(queryParams);

            return Ok(tags);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to load tags.");
            return StatusCode(500, ErrorResponse.Unexpected());
        }
    }

    /// <summary>
    /// Creates a reusable tag that can be attached to projects and blog posts.
    /// </summary>
    /// <param name="dto">The tag details to create.</param>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(UpsertTagDto dto)
    {
        try
        {
            TagDto tag = await _tagService.CreateAsync(dto);
            return CreatedAtAction(nameof(Get), new { search = tag.Name }, tag);
        }
        catch (ConflictException exception)
        {
            return StatusCode(409, ErrorResponse.Create(exception.Message));
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to create tag '{TagName}'.", dto.Name);
            return StatusCode(500, ErrorResponse.Unexpected());
        }
    }

    /// <summary>
    /// Updates administrator-managed metadata for an existing tag.
    /// </summary>
    /// <param name="tagId">The tag identifier.</param>
    /// <param name="dto">The tag details to save.</param>
    [HttpPut("{tagId:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int tagId, UpsertTagDto dto)
    {
        try
        {
            TagDto tag = await _tagService.UpdateAsync(tagId, dto);
            return Ok(tag);
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(ErrorResponse.Create(exception.Message));
        }
        catch (ConflictException exception)
        {
            return StatusCode(409, ErrorResponse.Create(exception.Message));
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to update tag {TagId}.", tagId);
            return StatusCode(500, ErrorResponse.Unexpected());
        }
    }

    /// <summary>
    /// Deletes a tag and removes its links from projects and blog posts.
    /// </summary>
    /// <param name="tagId">The tag identifier.</param>
    [HttpDelete("{tagId:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int tagId)
    {
        try
        {
            await _tagService.DeleteAsync(tagId);
            return NoContent();
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(ErrorResponse.Create(exception.Message));
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to delete tag {TagId}.", tagId);
            return StatusCode(500, ErrorResponse.Unexpected());
        }
    }
}
