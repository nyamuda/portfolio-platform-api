using System.ComponentModel.DataAnnotations;

namespace PortfolioPlatform.Api.Dtos.BlogPosts;

/// <summary>
/// Data required to create or update a blog post.
/// </summary>
public class UpsertBlogPostDto
{
    /// <summary>
    /// The title shown on post cards and the post details page.
    /// </summary>
    [Required]
    [StringLength(220, MinimumLength = 3)]
    public required string Title { get; set; }

    /// <summary>
    /// The URL-safe post slug used inside the owning profile's public route.
    /// </summary>
    [Required]
    [StringLength(140, MinimumLength = 3)]
    [RegularExpression("^[a-z0-9]+(?:-[a-z0-9]+)*$", ErrorMessage = "Slug must use lowercase letters, numbers, and hyphens only.")]
    public required string Slug { get; set; }

    /// <summary>
    /// A short preview that explains what the post helps the reader understand.
    /// </summary>
    [Required]
    [StringLength(500, MinimumLength = 10)]
    public required string Excerpt { get; set; }

    /// <summary>
    /// Rich HTML content from the frontend editor.
    /// </summary>
    public string? ContentHtml { get; set; }

    /// <summary>
    /// Plain-text content from the frontend editor.
    /// </summary>
    public string? ContentText { get; set; }

    /// <summary>
    /// Optional topic/category label.
    /// </summary>
    [StringLength(120)]
    public string? Category { get; set; }

    /// <summary>
    /// Tags connected to the post.
    /// </summary>
    public List<string> Tags { get; set; } = [];

    /// <summary>
    /// Public URL for the cover image uploaded by the frontend.
    /// </summary>
    [StringLength(500)]
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
    [StringLength(180)]
    public string? SeoTitle { get; set; }

    /// <summary>
    /// Optional browser/search description for the public post page.
    /// </summary>
    [StringLength(300)]
    public string? SeoDescription { get; set; }
}
