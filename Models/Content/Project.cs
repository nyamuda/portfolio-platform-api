using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using PortfolioPlatform.Api.Models.Profiles;

namespace PortfolioPlatform.Api.Models.Content;

/// <summary>
/// Project or case study displayed on a public profile.
/// </summary>
/// <remarks>
/// A project can represent software work, tutoring resources, creative work, professional case studies,
/// or any other piece of work the profile owner wants to present publicly.
/// </remarks>
[Index(nameof(ProfileId), nameof(Slug), IsUnique = true)]
public class Project
{
    /// <summary>
    /// Internal primary key for the project.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Foreign key of the profile that owns this project.
    /// </summary>
    public int ProfileId { get; set; }

    /// <summary>
    /// Profile that owns and displays this project.
    /// </summary>
    public Profile Profile { get; set; } = null!;

    /// <summary>
    /// Project title shown on cards and detail pages.
    /// </summary>
    [StringLength(180)]
    public required string Title { get; set; }

    /// <summary>
    /// URL-friendly identifier that is unique within the owning profile.
    /// </summary>
    /// <remarks>
    /// Slugs are scoped to a profile, so two different profiles can use the same project slug.
    /// </remarks>
    [StringLength(140)]
    public required string Slug { get; set; }

    /// <summary>
    /// Short summary used on project cards and previews.
    /// </summary>
    [StringLength(420)]
    public required string Summary { get; set; }

    /// <summary>
    /// Problem, need, or opportunity the project was built around.
    /// </summary>
    [StringLength(1200)]
    public string? Problem { get; set; }

    /// <summary>
    /// Solution, approach, or contribution delivered by the project.
    /// </summary>
    [StringLength(1600)]
    public string? Solution { get; set; }

    /// <summary>
    /// Rich project write-up stored as HTML from the frontend editor.
    /// </summary>
    public string? ContentHtml { get; set; }

    /// <summary>
    /// Plain text version of the write-up used for previews, search, and fallbacks.
    /// </summary>
    public string? ContentText { get; set; }

    /// <summary>
    /// Technologies, tools, skills, or subject areas connected to the project.
    /// </summary>
    public List<string> TechStack { get; set; } = [];

    /// <summary>
    /// Public URL of the cover image used for project previews.
    /// </summary>
    /// <remarks>
    /// The API stores only the final URL. File upload and storage are handled by the frontend.
    /// </remarks>
    public string? CoverImageUrl { get; set; }

    /// <summary>
    /// Public URLs of screenshots shown on the project detail page.
    /// </summary>
    /// <remarks>
    /// The API stores only final URLs. File upload and storage are handled by the frontend.
    /// </remarks>
    public List<string> ScreenshotUrls { get; set; } = [];

    /// <summary>
    /// Optional link to the live project, product, resource, demo, or case-study page.
    /// </summary>
    [StringLength(500)]
    public string? ProjectUrl { get; set; }

    /// <summary>
    /// Optional source repository URL.
    /// </summary>
    [StringLength(500)]
    public string? RepositoryUrl { get; set; }

    /// <summary>
    /// Manual ordering value used when displaying projects on a profile.
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// Indicates whether the project should be highlighted ahead of regular projects.
    /// </summary>
    public bool IsFeatured { get; set; }

    /// <summary>
    /// Indicates whether the project is visible to public visitors.
    /// </summary>
    /// <remarks>
    /// Draft projects remain available to the owner but are hidden from public endpoints.
    /// </remarks>
    public bool IsPublished { get; set; }

    /// <summary>
    /// Optional SEO title override for the public project page.
    /// </summary>
    [StringLength(180)]
    public string? SeoTitle { get; set; }

    /// <summary>
    /// Optional SEO description override for the public project page.
    /// </summary>
    [StringLength(320)]
    public string? SeoDescription { get; set; }

    /// <summary>
    /// Date and time when the project was created in UTC.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date and time when the project was last updated in UTC.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
