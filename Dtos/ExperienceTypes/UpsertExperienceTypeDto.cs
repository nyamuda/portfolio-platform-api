using System.ComponentModel.DataAnnotations;

namespace PortfolioPlatform.Api.Dtos.ExperienceTypes;

/// <summary>
/// Data required to create or update an experience type.
/// </summary>
public class UpsertExperienceTypeDto
{
    /// <summary>
    /// Display name shown in filters, forms, and timeline labels.
    /// </summary>
    [Required]
    [StringLength(120, MinimumLength = 2)]
    public required string Name { get; set; }

    /// <summary>
    /// Optional short explanation of what this type represents.
    /// </summary>
    [StringLength(500)]
    public string? Description { get; set; }

    /// <summary>
    /// Optional URL-friendly type slug. If omitted, the API generates it from the name.
    /// </summary>
    [StringLength(140)]
    public string? Slug { get; set; }

    /// <summary>
    /// Optional display color used for badges, chips, or timeline accents.
    /// </summary>
    [StringLength(20)]
    public string? ColorHex { get; set; }

    /// <summary>
    /// Optional icon name used by the frontend.
    /// </summary>
    [StringLength(80)]
    public string? IconName { get; set; }

    /// <summary>
    /// Whether this type should be highlighted in suggestions or public filters.
    /// </summary>
    public bool IsFeatured { get; set; }

    /// <summary>
    /// Manual ordering value for forms and public filters.
    /// </summary>
    public int SortOrder { get; set; }
}
