using System.ComponentModel.DataAnnotations;

namespace PortfolioPlatform.Api.Dtos.Tags;

/// <summary>
/// Request body used by administrators when creating or updating a tag.
/// </summary>
/// <remarks>
/// Usage counts are intentionally not included here. The API calculates those from the project and
/// blog-post relationships so clients cannot accidentally send stale or incorrect counts.
/// </remarks>
public class UpsertTagDto
{
    /// <summary>
    /// Display name of the tag, such as Vue, Teaching, UX, Chemistry, or ASP.NET Core.
    /// </summary>
    [Required]
    [StringLength(120)]
    public required string Name { get; set; }

    /// <summary>
    /// Optional short explanation of what this tag represents.
    /// </summary>
    [StringLength(500)]
    public string? Description { get; set; }

    /// <summary>
    /// Optional URL-friendly version of the tag name for public tag pages.
    /// </summary>
    /// <remarks>
    /// If this is left blank, the service creates a slug from <see cref="Name" />.
    /// </remarks>
    [StringLength(140)]
    public string? Slug { get; set; }

    /// <summary>
    /// Optional display color used for tag chips/badges in the frontend.
    /// </summary>
    [StringLength(20)]
    public string? ColorHex { get; set; }

    /// <summary>
    /// Optional icon name used by the frontend when showing richer tag cards.
    /// </summary>
    [StringLength(80)]
    public string? IconName { get; set; }

    /// <summary>
    /// Indicates whether this tag should be shown in public tag suggestions or featured tag areas.
    /// </summary>
    public bool IsFeatured { get; set; }
}
