using System.ComponentModel.DataAnnotations;

namespace PortfolioPlatform.Api.Dtos.Projects;

/// <summary>
/// Data required to create or update a portfolio project.
/// </summary>
public class UpsertProjectDto
{
    /// <summary>
    /// Project title shown on cards and detail pages.
    /// </summary>
    [Required]
    [StringLength(180, MinimumLength = 2)]
    public required string Title { get; set; }

    /// <summary>
    /// URL-friendly project identifier unique within the profile.
    /// </summary>
    [Required]
    [StringLength(140, MinimumLength = 3)]
    [RegularExpression(@"^[a-z0-9]+(?:-[a-z0-9]+)*$", ErrorMessage = "Slug must use lowercase letters, numbers, and hyphens.")]
    public required string Slug { get; set; }

    /// <summary>
    /// Short project summary used in previews.
    /// </summary>
    [Required]
    [StringLength(420, MinimumLength = 10)]
    public required string Summary { get; set; }

    /// <summary>
    /// Problem or opportunity the project was built around.
    /// </summary>
    [StringLength(1200)]
    public string? Problem { get; set; }

    /// <summary>
    /// Solution, approach, or contribution delivered by the project.
    /// </summary>
    [StringLength(1600)]
    public string? Solution { get; set; }

    /// <summary>
    /// Rich project write-up stored as sanitized HTML from the frontend editor.
    /// </summary>
    public string? ContentHtml { get; set; }

    /// <summary>
    /// Plain text version used for previews, search, or metadata.
    /// </summary>
    public string? ContentText { get; set; }

    /// <summary>
    /// Short labels for the project, such as tools, skills, subjects, or themes.
    /// </summary>
    public List<string> Tags { get; set; } = [];

    /// <summary>
    /// Cover image URL uploaded by the frontend.
    /// </summary>
    public string? CoverImageUrl { get; set; }

    /// <summary>
    /// Screenshot URLs uploaded by the frontend.
    /// </summary>
    public List<string> ScreenshotUrls { get; set; } = [];

    /// <summary>
    /// Optional live project or case-study URL.
    /// </summary>
    [StringLength(500)]
    public string? ProjectUrl { get; set; }

    /// <summary>
    /// Optional source repository URL.
    /// </summary>
    [StringLength(500)]
    public string? RepositoryUrl { get; set; }

    /// <summary>
    /// Manual display ordering value.
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// Whether the project should be highlighted.
    /// </summary>
    public bool IsFeatured { get; set; }

    /// <summary>
    /// Whether the project can be viewed publicly.
    /// </summary>
    public bool IsPublished { get; set; }

    /// <summary>
    /// Optional SEO title for the project page.
    /// </summary>
    [StringLength(180)]
    public string? SeoTitle { get; set; }

    /// <summary>
    /// Optional SEO description for the project page.
    /// </summary>
    [StringLength(320)]
    public string? SeoDescription { get; set; }
}

