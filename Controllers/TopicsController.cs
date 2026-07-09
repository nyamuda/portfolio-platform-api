using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortfolioPlatform.Api.Dtos.Topics;
using PortfolioPlatform.Api.Enums.Topics;
using PortfolioPlatform.Api.Exceptions;
using PortfolioPlatform.Api.Models;
using PortfolioPlatform.Api.Models.Content;
using PortfolioPlatform.Api.Services.Abstractions.Topics;

namespace PortfolioPlatform.Api.Controllers;

/// <summary>
/// Handles topic lookup for public screens and topic management for administrators.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class TopicsController(ITopicService topicService, ILogger<TopicsController> logger) : ControllerBase
{
    private readonly ITopicService _topicService = topicService;
    private readonly ILogger<TopicsController> _logger = logger;

    /// <summary>
    /// Gets a paginated list of blog topics.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Get(
        string? search = null,
        int page = 1,
        int pageSize = 10,
        TopicSortOption sortBy = TopicSortOption.Popularity
    )
    {
        try
        {
            TopicQueryParams queryParams = new()
            {
                Page = page,
                PageSize = pageSize,
                SortBy = sortBy,
                Search = search
            };

            PageInfo<TopicDto> topics = await _topicService.GetTopicsAsync(queryParams);

            return Ok(topics);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to load topics.");
            return StatusCode(500, ErrorResponse.Unexpected());
        }
    }

    /// <summary>
    /// Creates a topic that can be assigned to blog posts.
    /// </summary>
    /// <param name="dto">The topic details to create.</param>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(UpsertTopicDto dto)
    {
        try
        {
            TopicDto topic = await _topicService.CreateAsync(dto);
            return CreatedAtAction(nameof(Get), new { search = topic.Name }, topic);
        }
        catch (ConflictException exception)
        {
            return StatusCode(409, ErrorResponse.Create(exception.Message));
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to create topic '{TopicName}'.", dto.Name);
            return StatusCode(500, ErrorResponse.Unexpected());
        }
    }

    /// <summary>
    /// Updates administrator-managed metadata for an existing topic.
    /// </summary>
    /// <param name="topicId">The topic identifier.</param>
    /// <param name="dto">The topic details to save.</param>
    [HttpPut("{topicId:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int topicId, UpsertTopicDto dto)
    {
        try
        {
            TopicDto topic = await _topicService.UpdateAsync(topicId, dto);
            return Ok(topic);
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
            _logger.LogError(exception, "Failed to update topic {TopicId}.", topicId);
            return StatusCode(500, ErrorResponse.Unexpected());
        }
    }

    /// <summary>
    /// Deletes a topic and leaves its blog posts in place.
    /// </summary>
    /// <param name="topicId">The topic identifier.</param>
    [HttpDelete("{topicId:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int topicId)
    {
        try
        {
            await _topicService.DeleteAsync(topicId);
            return NoContent();
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(ErrorResponse.Create(exception.Message));
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to delete topic {TopicId}.", topicId);
            return StatusCode(500, ErrorResponse.Unexpected());
        }
    }
}
