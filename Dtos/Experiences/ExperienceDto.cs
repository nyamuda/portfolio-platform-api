using PortfolioPlatform.Api.Enums.Experiences;

namespace PortfolioPlatform.Api.Dtos.Experiences;

/// <summary>
/// Experience details returned by owner and public experience endpoints.
/// </summary>
public class ExperienceDto
{
    /// <summary>
    /// The unique database identifier for the experience.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The profile that owns and displays this experience.
    /// </summary>
    public int ProfileId { get; set; }

    /// <summary>
    /// The selected primary experience type id.
    /// </summary>
    public int ExperienceTypeId { get; set; }

    /// <summary>
    /// The selected primary experience type name.
    /// </summary>
    public required string ExperienceTypeName { get; set; }

    /// <summary>
    /// The selected primary experience type slug.
    /// </summary>
    public string? ExperienceTypeSlug { get; set; }

    /// <summary>
    /// The selected primary experience type color.
    /// </summary>
    public string? ExperienceTypeColorHex { get; set; }

    /// <summary>
    /// The selected primary experience type icon.
    /// </summary>
    public string? ExperienceTypeIconName { get; set; }

    /// <summary>
    /// Main title shown for this timeline entry.
    /// </summary>
    public required string Title { get; set; }

    /// <summary>
    /// Optional organisation, company, client, school, or community connected to the experience.
    /// </summary>
    public string? Organization { get; set; }

    /// <summary>
    /// Optional location or delivery context.
    /// </summary>
    public string? Location { get; set; }

    /// <summary>
    /// Where or how the experience took place.
    /// </summary>
    public ExperienceMode Mode { get; set; }

    /// <summary>
    /// Time or workload commitment connected to the experience.
    /// </summary>
    public ExperienceCommitment Commitment { get; set; }

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
    public string? Summary { get; set; }

    /// <summary>
    /// Rich HTML description for the timeline entry.
    /// </summary>
    public string? DescriptionHtml { get; set; }

    /// <summary>
    /// Plain text version of the description.
    /// </summary>
    public string? DescriptionText { get; set; }

    /// <summary>
    /// Optional public link connected to this experience.
    /// </summary>
    public string? ExternalUrl { get; set; }

    /// <summary>
    /// Short labels that help visitors understand what this experience is connected to.
    /// </summary>
    public List<string> Tags { get; set; } = [];

    /// <summary>
    /// Manual ordering value used to arrange experiences on a profile.
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// Whether this experience should be highlighted ahead of regular experiences.
    /// </summary>
    public bool IsFeatured { get; set; }

    /// <summary>
    /// Whether this experience is visible on the public profile.
    /// </summary>
    public bool IsPublished { get; set; }

    /// <summary>
    /// When the experience was first created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// When the experience was last updated, if it has been edited after creation.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}

