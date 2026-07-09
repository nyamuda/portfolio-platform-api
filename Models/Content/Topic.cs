using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using PortfolioPlatform.Api.Models.Content;

namespace PortfolioPlatform.Api.Models.Content;

/// <summary>
/// Managed writing topic used to group blog posts on public profile and blog pages.
/// </summary>
/// <remarks>
/// Topics are broader than tags. A blog post should usually have one primary topic, such as
/// Development, Teaching, Product, Career, or UX, while tags can still describe the smaller tools,
/// skills, or ideas connected to the post.
/// </remarks>
[Index(nameof(Name), IsUnique = true)]
[Index(nameof(Slug), IsUnique = true)]
public class Topic
{
    /// <summary>
    /// Internal primary key for the topic.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Display name shown to writers and public visitors.
    /// </summary>
    [Required]
    [StringLength(120)]
    public required string Name { get; set; }

    /// <summary>
    /// Optional short explanation of the kind of posts that belong to this topic.
    /// </summary>
    [StringLength(500)]
    public string? Description { get; set; }

    /// <summary>
    /// URL-friendly version of the topic name for future public topic pages.
    /// </summary>
    [StringLength(140)]
    public string? Slug { get; set; }

    /// <summary>
    /// Optional display color used by the frontend for topic chips, cards, or sidebars.
    /// </summary>
    [StringLength(20)]
    public string? ColorHex { get; set; }

    /// <summary>
    /// Optional icon name used by the frontend when showing richer topic cards.
    /// </summary>
    [StringLength(80)]
    public string? IconName { get; set; }

    /// <summary>
    /// Indicates whether this topic should appear in featured or popular topic areas.
    /// </summary>
    public bool IsFeatured { get; set; }

    /// <summary>
    /// Blog posts that use this topic as their primary grouping.
    /// </summary>
    public List<BlogPost> BlogPosts { get; set; } = [];

    /// <summary>
    /// Date and time when the topic was first created in UTC.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date and time when the topic was last updated in UTC.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
