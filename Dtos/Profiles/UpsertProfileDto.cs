using System.ComponentModel.DataAnnotations;

namespace PortfolioPlatform.Api.Dtos.Profiles;

/// <summary>
/// Data required to create or update the authenticated user's public profile.
/// </summary>
public class UpsertProfileDto
{
    /// <summary>
    /// Name shown at the top of the public profile.
    /// </summary>
    [Required]
    [StringLength(160, MinimumLength = 2)]
    public required string DisplayName { get; set; }

    /// <summary>
    /// URL-friendly public profile identifier.
    /// </summary>
    [Required]
    [StringLength(120, MinimumLength = 3)]
    [RegularExpression(@"^[a-z0-9]+(?:-[a-z0-9]+)*$", ErrorMessage = "Slug must use lowercase letters, numbers, and hyphens.")]
    public required string Slug { get; set; }

    /// <summary>
    /// Short professional or creator headline.
    /// </summary>
    [Required]
    [StringLength(220, MinimumLength = 3)]
    public required string Headline { get; set; }

    /// <summary>
    /// Short supporting line shown near the profile headline.
    /// </summary>
    public string? Tagline { get; set; }
    /// <summary>
    /// Short plain-text profile summary used in hero sections, cards, and compact previews.
    /// </summary>
    public string? Bio { get; set; }

    /// <summary>
    /// Plain-text version of the profile about section.
    /// </summary>
    public string? AboutContentText { get; set; }

    /// <summary>
    /// Rich HTML version of the profile about section.
    /// </summary>
    public string? AboutContentHtml { get; set; }

    /// <summary>
    /// Optional text describing what the person is currently working on.
    /// </summary>
    public string? CurrentFocus { get; set; }

    /// <summary>
    /// Profile image URL uploaded by the frontend.
    /// </summary>
    public string? ProfileImageUrl { get; set; }

    /// <summary>
    /// Cover image URL uploaded by the frontend.
    /// </summary>
    public string? CoverImageUrl { get; set; }

    /// <summary>
    /// Optional location shown publicly.
    /// </summary>
    [StringLength(160)]
    public string? Location { get; set; }

    /// <summary>
    /// Optional website URL.
    /// </summary>
    [StringLength(500)]
    public string? WebsiteUrl { get; set; }

    /// <summary>
    /// Optional GitHub profile URL.
    /// </summary>
    [StringLength(500)]
    public string? GitHubUrl { get; set; }

    /// <summary>
    /// Optional LinkedIn profile URL.
    /// </summary>
    [StringLength(500)]
    public string? LinkedInUrl { get; set; }

    /// <summary>
    /// Optional X/Twitter profile URL.
    /// </summary>
    [StringLength(500)]
    public string? XUrl { get; set; }

    /// <summary>
    /// Optional SEO title for the public profile page.
    /// </summary>
    [StringLength(180)]
    public string? SeoTitle { get; set; }

    /// <summary>
    /// Optional SEO description for the public profile page.
    /// </summary>
    [StringLength(320)]
    public string? SeoDescription { get; set; }

    /// <summary>
    /// Whether the profile can be viewed publicly.
    /// </summary>
    public bool IsPublished { get; set; }
}


