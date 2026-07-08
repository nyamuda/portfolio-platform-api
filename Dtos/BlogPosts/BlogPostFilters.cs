using PortfolioPlatform.Api.Enums.BlogPosts;

namespace PortfolioPlatform.Api.Dtos.BlogPosts;

/// <summary>
/// Filter values accepted by the owner-facing blog post list endpoint.
/// </summary>
public class BlogPostFilters
{
    /// <summary>
    /// Optional text used to match post title, excerpt, category, or content text.
    /// </summary>
    public string? SearchTerm { get; set; }

    /// <summary>
    /// Publication state to include in the result set.
    /// </summary>
    public BlogPostStatus Status { get; set; } = BlogPostStatus.All;

    /// <summary>
    /// Featured-state filter to apply to the result set.
    /// </summary>
    public BlogPostFeaturedFilter Featured { get; set; } = BlogPostFeaturedFilter.All;

    /// <summary>
    /// Sort order used for the returned blog post list.
    /// </summary>
    public BlogPostSortOption SortBy { get; set; } = BlogPostSortOption.Recent;

    /// <summary>
    /// One-based page number requested by the blog post list paginator.
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// Number of posts requested per page. The service clamps this value to keep requests sensible.
    /// </summary>
    public int PageSize { get; set; } = 12;
}
