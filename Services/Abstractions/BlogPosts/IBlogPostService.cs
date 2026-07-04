using PortfolioPlatform.Api.Dtos.BlogPosts;

namespace PortfolioPlatform.Api.Services.Abstractions.BlogPosts;

/// <summary>
/// Provides blog post operations for authenticated owners and public visitors.
/// </summary>
public interface IBlogPostService
{
    /// <summary>
    /// Returns all blog posts owned by the authenticated user, including drafts.
    /// </summary>
    /// <param name="userId">The authenticated user's identifier.</param>
    /// <returns>Blog posts owned by the user's profile.</returns>
    Task<List<BlogPostDto>> GetMineAsync(int userId);

    /// <summary>
    /// Returns one blog post owned by the authenticated user.
    /// </summary>
    /// <param name="userId">The authenticated user's identifier.</param>
    /// <param name="postId">The blog post identifier.</param>
    /// <returns>The requested owner-facing blog post.</returns>
    Task<BlogPostDto> GetMineByIdAsync(int userId, int postId);

    /// <summary>
    /// Returns published blog posts for a published public profile.
    /// </summary>
    /// <param name="profileSlug">The public profile slug.</param>
    /// <returns>Published blog posts for the profile.</returns>
    Task<List<BlogPostDto>> GetPublicByProfileSlugAsync(string profileSlug);

    /// <summary>
    /// Returns one published blog post from a published public profile.
    /// </summary>
    /// <param name="profileSlug">The public profile slug.</param>
    /// <param name="postSlug">The public blog post slug.</param>
    /// <returns>The published blog post.</returns>
    Task<BlogPostDto> GetPublicBySlugAsync(string profileSlug, string postSlug);

    /// <summary>
    /// Creates a blog post for the authenticated user's profile.
    /// </summary>
    /// <param name="userId">The authenticated user's identifier.</param>
    /// <param name="dto">The blog post details to create.</param>
    /// <returns>The created blog post.</returns>
    Task<BlogPostDto> CreateAsync(int userId, UpsertBlogPostDto dto);

    /// <summary>
    /// Updates a blog post owned by the authenticated user's profile.
    /// </summary>
    /// <param name="userId">The authenticated user's identifier.</param>
    /// <param name="postId">The blog post identifier.</param>
    /// <param name="dto">The blog post details to save.</param>
    /// <returns>The updated blog post.</returns>
    Task<BlogPostDto> UpdateAsync(int userId, int postId, UpsertBlogPostDto dto);

    /// <summary>
    /// Deletes a blog post owned by the authenticated user's profile.
    /// </summary>
    /// <param name="userId">The authenticated user's identifier.</param>
    /// <param name="postId">The blog post identifier.</param>
    Task DeleteAsync(int userId, int postId);
}

