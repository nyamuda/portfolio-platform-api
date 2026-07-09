using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using PortfolioPlatform.Api.Models.Profiles;

namespace PortfolioPlatform.Api.Models.Content;

/// <summary>
/// Written post displayed on a public profile.
/// </summary>
/// <remarks>
/// A blog post is a single article or note. The model name stays specific because "Blog" usually
/// describes the whole collection, while this record represents one published or draft post.
/// </remarks>
[Index(nameof(ProfileId), nameof(Slug), IsUnique = true)]
public class BlogPost
{
    /// <summary>
    /// Internal primary key for the post.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Foreign key of the profile that owns this post.
    /// </summary>
    public int ProfileId { get; set; }

    /// <summary>
    /// Profile that owns and displays this post.
    /// </summary>
    public Profile Profile { get; set; } = null!;

    /// <summary>
    /// Title shown on post cards and the post detail page.
    /// </summary>
    [Required]
    [StringLength(220)]
    public required string Title { get; set; }

    /// <summary>
    /// URL-friendly identifier that is unique within the owning profile.
    /// </summary>
    /// <remarks>
    /// Slugs are scoped to a profile, so two different profiles can use the same post slug.
    /// </remarks>
    [Required]
    [StringLength(140)]
    public required string Slug { get; set; }

    /// <summary>
    /// Short preview that helps visitors decide whether to open the post.
    /// </summary>
    [Required]
    [StringLength(500)]
    public required string Excerpt { get; set; }

    /// <summary>
    /// Rich post body stored as HTML from the frontend editor.
    /// </summary>
    public string? ContentHtml { get; set; }

    /// <summary>
    /// Plain-text version of the post body used for previews, search, and fallbacks.
    /// </summary>
    public string? ContentText { get; set; }


    /// <summary>
    /// Optional managed topic used as the post's primary grouping.
    /// </summary>
    public int? TopicId { get; set; }

    /// <summary>
    /// Managed topic assigned to this post, when one has been selected.
    /// </summary>
    public Topic? Topic { get; set; }

    /// <summary>
    /// Tags connected to the post.
    /// </summary>
    public List<Tag> Tags { get; set; } = [];

    /// <summary>
    /// Public URL of the cover image uploaded by the frontend.
    /// </summary>
    /// <remarks>
    /// The API stores only the final URL. File upload and storage are handled by the frontend.
    /// </remarks>
    [StringLength(500)]
    public string? CoverImageUrl { get; set; }

    /// <summary>
    /// Manual ordering value used when featured posts are displayed.
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// Indicates whether the post should be highlighted ahead of regular posts.
    /// </summary>
    public bool IsFeatured { get; set; }

    /// <summary>
    /// Indicates whether the post is visible to public visitors.
    /// </summary>
    /// <remarks>
    /// Draft posts remain available to the owner but are hidden from public endpoints.
    /// </remarks>
    public bool IsPublished { get; set; }

    /// <summary>
    /// Optional SEO title override for the public post page.
    /// </summary>
    [StringLength(180)]
    public string? SeoTitle { get; set; }

    /// <summary>
    /// Optional SEO description override for the public post page.
    /// </summary>
    [StringLength(300)]
    public string? SeoDescription { get; set; }

    /// <summary>
    /// Date and time when the post was created in UTC.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date and time when the post was last updated in UTC.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}

