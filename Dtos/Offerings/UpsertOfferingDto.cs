using System.ComponentModel.DataAnnotations;

namespace PortfolioPlatform.Api.Dtos.Offerings;

/// <summary>
/// Data required to create or update a public offering.
/// </summary>
public class UpsertOfferingDto
{
    /// <summary>
    /// Offering title shown on cards and detail pages.
    /// </summary>
    [Required]
    [StringLength(180, MinimumLength = 2)]
    public required string Title { get; set; }

    /// <summary>
    /// URL-friendly offering identifier unique within the profile.
    /// </summary>
    [Required]
    [StringLength(140, MinimumLength = 3)]
    [RegularExpression(@"^[a-z0-9]+(?:-[a-z0-9]+)*$", ErrorMessage = "Slug must use lowercase letters, numbers, and hyphens.")]
    public required string Slug { get; set; }

    /// <summary>
    /// Short explanation used in previews.
    /// </summary>
    [Required]
    [StringLength(420, MinimumLength = 10)]
    public required string Summary { get; set; }

    /// <summary>
    /// Rich offering details stored as sanitized HTML from the frontend editor.
    /// </summary>
    public string? ContentHtml { get; set; }

    /// <summary>
    /// Plain text version used for previews, search, or metadata.
    /// </summary>
    public string? ContentText { get; set; }

    /// <summary>
    /// Optional price label shown to visitors.
    /// </summary>
    [StringLength(160)]
    public string? Price { get; set; }

    /// <summary>
    /// Optional delivery label shown to visitors.
    /// </summary>
    [StringLength(160)]
    public string? DeliveryMode { get; set; }

    /// <summary>
    /// Optional duration label shown to visitors.
    /// </summary>
    [StringLength(160)]
    public string? Duration { get; set; }

    /// <summary>
    /// Optional call-to-action label.
    /// </summary>
    [StringLength(120)]
    public string? CallToAction { get; set; }

    /// <summary>
    /// Optional call-to-action link.
    /// </summary>
    [StringLength(500)]
    public string? CallToActionUrl { get; set; }

    /// <summary>
    /// Short labels for the offering, such as tools, skills, subjects, or audience needs.
    /// </summary>
    public List<string> Tags { get; set; } = [];

    /// <summary>
    /// Manual display ordering value.
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// Whether the offering should be highlighted.
    /// </summary>
    public bool IsFeatured { get; set; }

    /// <summary>
    /// Whether the offering can be viewed publicly.
    /// </summary>
    public bool IsPublished { get; set; }

    /// <summary>
    /// Optional SEO title for the offering page.
    /// </summary>
    [StringLength(180)]
    public string? SeoTitle { get; set; }

    /// <summary>
    /// Optional SEO description for the offering page.
    /// </summary>
    [StringLength(320)]
    public string? SeoDescription { get; set; }
}

