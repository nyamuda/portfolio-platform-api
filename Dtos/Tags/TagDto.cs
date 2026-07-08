namespace PortfolioPlatform.Api.Dtos.Tags;

/// <summary>
/// Tag information returned to the frontend.
/// </summary>
public class TagDto
{
    /// <summary>
    /// Internal primary key for the tag.
    /// </summary>
    public required int Id { get; set; }

    /// <summary>
    /// Display name of the tag.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Optional short explanation of what this tag represents.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Optional URL-friendly version of the tag name for public tag pages.
    /// </summary>
    public string? Slug { get; set; }

    /// <summary>
    /// Optional display color used for tag chips/badges in the frontend.
    /// </summary>
    public string? ColorHex { get; set; }

    /// <summary>
    /// Optional icon name used by the frontend when showing richer tag cards.
    /// </summary>
    public string? IconName { get; set; }

    /// <summary>
    /// Indicates whether this tag should be shown in public tag suggestions or featured tag areas.
    /// </summary>
    public bool IsFeatured { get; set; }

    /// <summary>
    /// Date and time when the tag was first created in UTC.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Date and time when the tag was last updated in UTC.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Number of projects that currently use this tag.
    /// </summary>
    public int TotalProjects { get; set; }

    /// <summary>
    /// Number of blog posts that currently use this tag.
    /// </summary>
    public int TotalBlogPosts { get; set; }

    /// <summary>
    /// Combined number of projects and blog posts that use this tag.
    /// </summary>
    public int TotalUses { get; set; }
}
