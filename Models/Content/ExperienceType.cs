using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace PortfolioPlatform.Api.Models.Content;

/// <summary>
/// Reusable category that explains the primary kind of timeline entry.
/// </summary>
/// <remarks>
/// Experience types are shared vocabulary for timeline entries. Examples include Work,
/// Education, Volunteering, Teaching, Certification, Award, Speaking, and Milestone.
/// One type can be reused by many experiences across many profiles.
/// </remarks>
[Index(nameof(Name), IsUnique = true)]
[Index(nameof(Slug), IsUnique = true)]
public class ExperienceType
{
    /// <summary>
    /// Internal primary key for the experience type.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Display name shown in filters, forms, and timeline labels.
    /// </summary>
    [StringLength(120)]
    public required string Name { get; set; }

    /// <summary>
    /// Optional short explanation of what this experience type represents.
    /// </summary>
    [StringLength(500)]
    public string? Description { get; set; }

    /// <summary>
    /// Optional URL-friendly version of the type name for future public type pages.
    /// </summary>
    [StringLength(140)]
    public string? Slug { get; set; }

    /// <summary>
    /// Optional display color used for badges, chips, or timeline accents in the frontend.
    /// </summary>
    [StringLength(20)]
    public string? ColorHex { get; set; }

    /// <summary>
    /// Optional icon name used by the frontend when showing richer experience type labels.
    /// </summary>
    [StringLength(80)]
    public string? IconName { get; set; }

    /// <summary>
    /// Indicates whether this type should be highlighted in public filters or admin suggestions.
    /// </summary>
    public bool IsFeatured { get; set; }

    /// <summary>
    /// Manual ordering value for forms and public filters.
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// Experiences that use this type as their primary classification.
    /// </summary>
    public List<Experience> Experiences { get; set; } = [];

    /// <summary>
    /// Date and time when the type was first created in UTC.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date and time when the type was last updated in UTC.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
