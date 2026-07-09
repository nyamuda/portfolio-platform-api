using PortfolioPlatform.Api.Dtos.Topics;
using PortfolioPlatform.Api.Models;
using PortfolioPlatform.Api.Models.Content;

namespace PortfolioPlatform.Api.Services.Abstractions.Topics;

/// <summary>
/// Handles managed blog topic lookup, listing, creation, update, and deletion.
/// </summary>
public interface ITopicService
{
    /// <summary>
    /// Retrieves a paginated list of topics with blog-post usage counts.
    /// </summary>
    /// <param name="queryParams">Search, sorting, and pagination values for the request.</param>
    /// <returns>A paginated list of topics.</returns>
    Task<PageInfo<TopicDto>> GetTopicsAsync(TopicQueryParams queryParams);

    /// <summary>
    /// Creates a new blog topic from administrator-managed metadata.
    /// </summary>
    /// <param name="dto">The topic details to create.</param>
    /// <returns>The newly created topic.</returns>
    Task<TopicDto> CreateAsync(UpsertTopicDto dto);

    /// <summary>
    /// Updates an existing blog topic and keeps its public metadata current.
    /// </summary>
    /// <param name="topicId">The topic identifier.</param>
    /// <param name="dto">The topic details to save.</param>
    /// <returns>The updated topic.</returns>
    Task<TopicDto> UpdateAsync(int topicId, UpsertTopicDto dto);

    /// <summary>
    /// Deletes a topic and removes its blog-post links without deleting the posts.
    /// </summary>
    /// <param name="topicId">The topic identifier.</param>
    Task DeleteAsync(int topicId);
}
