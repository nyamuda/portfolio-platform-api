using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using PortfolioPlatform.Api.Models.Profiles;

namespace PortfolioPlatform.Api.Models.Content;

/// <summary>
/// Public offering that explains a service, package, session, or type of work a profile owner provides.
/// </summary>
/// <remarks>
/// Offerings are intentionally broader than technical services. A creator might offer tutoring,
/// consultation, coaching, design work, writing, code reviews, workshops, or any other structured way
/// visitors can work with them.
/// </remarks>
[Index(nameof(ProfileId), nameof(Slug), IsUnique = true)]
public class Offering
{
    /// <summary>
    /// Internal primary key for the offering.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Foreign key of the profile that owns this offering.
    /// </summary>
    public int ProfileId { get; set; }

    /// <summary>
    /// Profile that owns and displays this offering.
    /// </summary>
    public Profile Profile { get; set; } = null!;

    /// <summary>
    /// Offering title shown on cards and detail pages.
    /// </summary>
    [StringLength(180)]
    public required string Title { get; set; }

    /// <summary>
    /// URL-friendly identifier that is unique within the owning profile.
    /// </summary>
    /// <remarks>
    /// Slugs are scoped to a profile, so two different profiles can use the same offering slug.
    /// </remarks>
    [StringLength(140)]
    public required string Slug { get; set; }

    /// <summary>
    /// Short explanation used on offering cards and previews.
    /// </summary>
    [StringLength(420)]
    public required string Summary { get; set; }

    /// <summary>
    /// Rich offering details stored as HTML from the frontend editor.
    /// </summary>
    public string? ContentHtml { get; set; }

    /// <summary>
    /// Plain text version of the offering details used for previews, search, and fallbacks.
    /// </summary>
    public string? ContentText { get; set; }

    /// <summary>
    /// Optional price label such as Free consultation, From $50, Project-based, or Contact for pricing.
    /// </summary>
    [StringLength(160)]
    public string? Price { get; set; }

    /// <summary>
    /// Optional delivery label such as Online, In person, Hybrid, Remote, or Self-paced.
    /// </summary>
    [StringLength(160)]
    public string? DeliveryMode { get; set; }

    /// <summary>
    /// Optional duration label such as 60 minutes, 2 weeks, Monthly, or Flexible.
    /// </summary>
    [StringLength(160)]
    public string? Duration { get; set; }

    /// <summary>
    /// Optional button label shown on the public offering card or detail page.
    /// </summary>
    [StringLength(120)]
    public string? CallToAction { get; set; }

    /// <summary>
    /// Optional link used when the visitor clicks the offering call-to-action.
    /// </summary>
    [StringLength(500)]
    public string? CallToActionUrl { get; set; }

    /// <summary>
    /// Short labels that describe the offering, such as skills, subjects, tools, or audience needs.
    /// </summary>
    public List<Tag> Tags { get; set; } = [];

    /// <summary>
    /// Manual ordering value used when displaying offerings on a profile.
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// Indicates whether the offering should be highlighted ahead of regular offerings.
    /// </summary>
    public bool IsFeatured { get; set; }

    /// <summary>
    /// Indicates whether the offering is visible to public visitors.
    /// </summary>
    /// <remarks>
    /// Draft offerings remain available to the owner but are hidden from public endpoints.
    /// </remarks>
    public bool IsPublished { get; set; }

    /// <summary>
    /// Optional SEO title override for the public offering page.
    /// </summary>
    [StringLength(180)]
    public string? SeoTitle { get; set; }

    /// <summary>
    /// Optional SEO description override for the public offering page.
    /// </summary>
    [StringLength(320)]
    public string? SeoDescription { get; set; }

    /// <summary>
    /// Date and time when the offering was created in UTC.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date and time when the offering was last updated in UTC.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}

