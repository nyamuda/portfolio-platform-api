using PortfolioPlatform.Api.Dtos.Tags;
using PortfolioPlatform.Api.Models;
using PortfolioPlatform.Api.Models.Content;

namespace PortfolioPlatform.Api.Services.Abstractions.Tags;

/// <summary>
/// Handles reusable tag lookup, creation, and listing for portfolio content.
/// </summary>
public interface ITagService
{
    /// <summary>
    /// Gets a tag by name, creating it when it does not already exist.
    /// </summary>
    /// <param name="name">The tag name to find or create.</param>
    /// <returns>The existing or newly created tag.</returns>
    Task<Tag> GetByNameAsync(string name);

    /// <summary>
    /// Ensures each supplied tag name exists in the central tag table.
    /// </summary>
    /// <param name="tagNames">Tag names collected from a project, blog post, or profile field.</param>
    Task EnsureTagsExistAsync(IEnumerable<string> tagNames);

    /// <summary>
    /// Retrieves a paginated list of tags with usage counts across projects and blog posts.
    /// </summary>
    /// <param name="queryParams">Search, sorting, and pagination values for the request.</param>
    /// <returns>A paginated list of tags.</returns>
    Task<PageInfo<TagDto>> GetTagsAsync(TagQueryParams queryParams);

    /// <summary>
    /// Creates a tag from administrator-managed metadata.
    /// </summary>
    /// <param name="dto">The tag details to create.</param>
    /// <returns>The newly created tag.</returns>
    Task<TagDto> CreateAsync(UpsertTagDto dto);

    /// <summary>
    /// Updates an existing tag and keeps its public metadata current.
    /// </summary>
    /// <param name="tagId">The tag identifier.</param>
    /// <param name="dto">The tag details to save.</param>
    /// <returns>The updated tag.</returns>
    Task<TagDto> UpdateAsync(int tagId, UpsertTagDto dto);

    /// <summary>
    /// Deletes a tag and removes its project/blog-post links.
    /// </summary>
    /// <param name="tagId">The tag identifier.</param>
    Task DeleteAsync(int tagId);}

