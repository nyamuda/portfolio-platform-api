using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using PortfolioPlatform.Api.Enums.Experiences;
using PortfolioPlatform.Api.Models.Profiles;

namespace PortfolioPlatform.Api.Models.Content;

/// <summary>
/// Timeline entry that describes a meaningful part of a person's background.
/// </summary>
/// <remarks>
/// An experience can represent work, education, volunteering, teaching, certification,
/// speaking, awards, or any other milestone that helps visitors understand the person's journey.
/// </remarks>
[Index(nameof(ProfileId))]
[Index(nameof(ExperienceTypeId))]
public class Experience
{
    /// <summary>
    /// Internal primary key for the experience.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Foreign key of the profile that owns this timeline entry.
    /// </summary>
    public int ProfileId { get; set; }

    /// <summary>
    /// Profile that owns and displays this experience.
    /// </summary>
    public Profile Profile { get; set; } = null!;

    /// <summary>
    /// Foreign key of the primary type for this experience.
    /// </summary>
    /// <remarks>
    /// Each experience has one primary type so the timeline can group or filter entries cleanly.
    /// For example, a single entry may be classified as Work, Education, Volunteering, or Certification.
    /// </remarks>
    public int ExperienceTypeId { get; set; }

    /// <summary>
    /// Primary type that explains what kind of experience this entry is.
    /// </summary>
    public ExperienceType ExperienceType { get; set; } = null!;

    /// <summary>
    /// Main title of the experience, such as Software Engineer, Chemistry Tutor, or BSc Computer Science.
    /// </summary>
    [StringLength(180)]
    public required string Title { get; set; }

    /// <summary>
    /// Optional organisation, school, client, company, or community connected to the experience.
    /// </summary>
    [StringLength(180)]
    public string? Organization { get; set; }

    /// <summary>
    /// Optional location or delivery context, such as Cape Town, Remote, Online, or Hybrid.
    /// </summary>
    [StringLength(180)]
    public string? Location { get; set; }

    /// <summary>
    /// Describes where or how the experience took place.
    /// </summary>
    /// <remarks>
    /// This is different from <see cref="ExperienceType"/>. For example, an entry can have the type
    /// Work and the mode Remote, or the type Teaching and the mode Online.
    /// </remarks>
    public ExperienceMode Mode { get; set; } = ExperienceMode.NotSpecified;

    /// <summary>
    /// Describes the time or workload commitment for the experience.
    /// </summary>
    /// <remarks>
    /// This is useful for values such as FullTime or PartTime without mixing them into experience categories.
    /// </remarks>
    public ExperienceCommitment Commitment { get; set; } = ExperienceCommitment.NotSpecified;

    /// <summary>
    /// Optional start date of the experience.
    /// </summary>
    public DateOnly? StartDate { get; set; }

    /// <summary>
    /// Optional end date of the experience.
    /// </summary>
    /// <remarks>
    /// Leave this empty when <see cref="IsCurrent"/> is true.
    /// </remarks>
    public DateOnly? EndDate { get; set; }

    /// <summary>
    /// Indicates whether this experience is still active.
    /// </summary>
    public bool IsCurrent { get; set; }

    /// <summary>
    /// Short summary used in timeline cards and compact previews.
    /// </summary>
    [StringLength(520)]
    public string? Summary { get; set; }

    /// <summary>
    /// Rich description stored as HTML from the frontend editor.
    /// </summary>
    public string? DescriptionHtml { get; set; }

    /// <summary>
    /// Plain text version of the rich description used for search, previews, and fallbacks.
    /// </summary>
    public string? DescriptionText { get; set; }

    /// <summary>
    /// Optional public link connected to this experience, such as a certificate, company page, or project page.
    /// </summary>
    [StringLength(500)]
    public string? ExternalUrl { get; set; }

    /// <summary>
    /// Reusable labels that add extra context to the experience.
    /// </summary>
    /// <remarks>
    /// Tags are secondary labels. The primary classification remains <see cref="ExperienceType"/>.
    /// For example, the type may be Work while tags might be ASP.NET Core, Vue, Teaching, or Leadership.
    /// </remarks>
    public List<Tag> Tags { get; set; } = [];

    /// <summary>
    /// Manual ordering value used when the owner wants to arrange experiences beyond date sorting.
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// Indicates whether this experience should be highlighted ahead of regular timeline entries.
    /// </summary>
    public bool IsFeatured { get; set; }

    /// <summary>
    /// Indicates whether this experience is visible to public visitors.
    /// </summary>
    public bool IsPublished { get; set; }

    /// <summary>
    /// Date and time when the experience was created in UTC.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date and time when the experience was last updated in UTC.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}


