using System.ComponentModel.DataAnnotations;
using PortfolioPlatform.Api.Enums.Experiences;

namespace PortfolioPlatform.Api.Dtos.Experiences;

/// <summary>
/// Data required to create or update a timeline experience.
/// </summary>
public class UpsertExperienceDto
{
    /// <summary>
    /// Primary type selected for this timeline entry.
    /// </summary>
    [Required]
    public int ExperienceTypeId { get; set; }

    /// <summary>
    /// Main title of the experience.
    /// </summary>
    [Required]
    [StringLength(180, MinimumLength = 2)]
    public required string Title { get; set; }

    /// <summary>
    /// Optional organisation, company, client, school, or community connected to the experience.
    /// </summary>
    [StringLength(180)]
    public string? Organization { get; set; }

    /// <summary>
    /// Optional location or delivery context.
    /// </summary>
    [StringLength(180)]
    public string? Location { get; set; }

    /// <summary>
    /// Where or how the experience took place.
    /// </summary>
    public ExperienceMode Mode { get; set; } = ExperienceMode.NotSpecified;

    /// <summary>
    /// Time or workload commitment connected to the experience.
    /// </summary>
    public ExperienceCommitment Commitment { get; set; } = ExperienceCommitment.NotSpecified;

    /// <summary>
    /// Optional start date for the experience.
    /// </summary>
    public DateOnly? StartDate { get; set; }

    /// <summary>
    /// Optional end date for the experience.
    /// </summary>
    public DateOnly? EndDate { get; set; }

    /// <summary>
    /// Whether this experience is still active.
    /// </summary>
    public bool IsCurrent { get; set; }

    /// <summary>
    /// Short summary used on timeline cards and compact previews.
    /// </summary>
    [StringLength(520)]
    public string? Summary { get; set; }

    /// <summary>
    /// Rich experience details stored as sanitized HTML from the frontend editor.
    /// </summary>
    public string? DescriptionHtml { get; set; }

    /// <summary>
    /// Plain text version used for search, previews, and fallbacks.
    /// </summary>
    public string? DescriptionText { get; set; }

    /// <summary>
    /// Optional public link connected to this experience.
    /// </summary>
    [StringLength(500)]
    public string? ExternalUrl { get; set; }

    /// <summary>
    /// Short labels for the experience, such as tools, skills, subjects, or themes.
    /// </summary>
    public List<string> Tags { get; set; } = [];

    /// <summary>
    /// Manual display ordering value.
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// Whether the experience should be highlighted.
    /// </summary>
    public bool IsFeatured { get; set; }

    /// <summary>
    /// Whether the experience can be viewed publicly.
    /// </summary>
    public bool IsPublished { get; set; }
}


