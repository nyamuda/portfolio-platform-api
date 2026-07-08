using PortfolioPlatform.Api.Models.Content;

namespace PortfolioPlatform.Api.Dtos.BlogPosts;

/// <summary>
/// Blog post details returned by owner and public post endpoints.
/// </summary>
public class BlogPostDto
{
    /// <summary>
    /// The unique database identifier for the post.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The profile that owns and displays this post.
    /// </summary>
    public int ProfileId { get; set; }

    /// <summary>
    /// The post title.
    /// </summary>
    public required string Title { get; set; }

    /// <summary>
    /// The URL-safe post slug.
    /// </summary>
    public required string Slug { get; set; }

    /// <summary>
    /// A short preview of the post.
    /// </summary>
    public required string Excerpt { get; set; }

    /// <summary>
    /// Rich HTML content for the post body.
    /// </summary>
    public string? ContentHtml { get; set; }

    /// <summary>
    /// Plain-text version of the post body.
    /// </summary>
    public string? ContentText { get; set; }

    /// <summary>
    /// Optional topic/category label.
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    /// Tags connected to the post.
    /// </summary>
    public List<string> Tags { get; set; } = [];

    /// <summary>
    /// Public URL for the cover image uploaded by the frontend.
    /// </summary>
    public string? CoverImageUrl { get; set; }

    /// <summary>
    /// Manual ordering value used when featured posts are displayed.
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// Whether this post should be highlighted ahead of regular posts.
    /// </summary>
    public bool IsFeatured { get; set; }

    /// <summary>
    /// Whether this post is visible on the public profile.
    /// </summary>
    public bool IsPublished { get; set; }

    /// <summary>
    /// Optional browser/search title for the public post page.
    /// </summary>
    public string? SeoTitle { get; set; }

    /// <summary>
    /// Optional browser/search description for the public post page.
    /// </summary>
    public string? SeoDescription { get; set; }

    /// <summary>
    /// When the post was first created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// When the post was last updated, if it has been edited after creation.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Maps a post entity to a DTO.
    /// </summary>
    public static BlogPostDto MapFrom(BlogPost post)
    {
        return new BlogPostDto
        {
            Id = post.Id,
            ProfileId = post.ProfileId,
            Title = post.Title,
            Slug = post.Slug,
            Excerpt = post.Excerpt,
            ContentHtml = post.ContentHtml,
            ContentText = post.ContentText,
            Category = post.Category,
            Tags = post.Tags.Select(tag => tag.Name).ToList(),
            CoverImageUrl = post.CoverImageUrl,
            SortOrder = post.SortOrder,
            IsFeatured = post.IsFeatured,
            IsPublished = post.IsPublished,
            SeoTitle = post.SeoTitle,
            SeoDescription = post.SeoDescription,
            CreatedAt = post.CreatedAt,
            UpdatedAt = post.UpdatedAt
        };
    }
}

