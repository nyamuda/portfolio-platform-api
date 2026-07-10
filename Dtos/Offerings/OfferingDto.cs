using PortfolioPlatform.Api.Models.Content;

namespace PortfolioPlatform.Api.Dtos.Offerings;

/// <summary>
/// Offering details returned by owner and public offering endpoints.
/// </summary>
public class OfferingDto
{
    /// <summary>
    /// The unique database identifier for the offering.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The profile that owns and displays this offering.
    /// </summary>
    public int ProfileId { get; set; }

    /// <summary>
    /// The public title shown on offering cards and the offering details page.
    /// </summary>
    public required string Title { get; set; }

    /// <summary>
    /// The URL-safe offering slug used inside the owning profile's public route.
    /// </summary>
    public required string Slug { get; set; }

    /// <summary>
    /// A short description of what the offering is and who it helps.
    /// </summary>
    public required string Summary { get; set; }

    /// <summary>
    /// Rich HTML content for the full offering description.
    /// </summary>
    public string? ContentHtml { get; set; }

    /// <summary>
    /// Plain-text version of the offering description for search, previews, and fallbacks.
    /// </summary>
    public string? ContentText { get; set; }

    /// <summary>
    /// Optional price label shown to visitors.
    /// </summary>
    public string? Price { get; set; }

    /// <summary>
    /// Optional delivery label shown to visitors.
    /// </summary>
    public string? DeliveryMode { get; set; }

    /// <summary>
    /// Optional duration label shown to visitors.
    /// </summary>
    public string? Duration { get; set; }

    /// <summary>
    /// Optional call-to-action label.
    /// </summary>
    public string? CallToAction { get; set; }

    /// <summary>
    /// Optional call-to-action link.
    /// </summary>
    public string? CallToActionUrl { get; set; }

    /// <summary>
    /// Short labels that help visitors understand what the offering is connected to.
    /// </summary>
    public List<string> Tags { get; set; } = [];

    /// <summary>
    /// Manual ordering value used to arrange offerings on a profile.
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// Whether this offering should be highlighted ahead of regular offerings.
    /// </summary>
    public bool IsFeatured { get; set; }

    /// <summary>
    /// Whether this offering is visible on the public profile.
    /// </summary>
    public bool IsPublished { get; set; }

    /// <summary>
    /// Optional browser/search title for the public offering page.
    /// </summary>
    public string? SeoTitle { get; set; }

    /// <summary>
    /// Optional browser/search description for the public offering page.
    /// </summary>
    public string? SeoDescription { get; set; }

    /// <summary>
    /// When the offering was first created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// When the offering was last updated, if it has been edited after creation.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Maps an offering entity to a DTO.
    /// </summary>
    /// <param name="offering">The offering entity to convert.</param>
    /// <returns>The API response DTO.</returns>
    public static OfferingDto MapFrom(Offering offering)
    {
        return new OfferingDto
        {
            Id = offering.Id,
            ProfileId = offering.ProfileId,
            Title = offering.Title,
            Slug = offering.Slug,
            Summary = offering.Summary,
            ContentHtml = offering.ContentHtml,
            ContentText = offering.ContentText,
            Price = offering.Price,
            DeliveryMode = offering.DeliveryMode,
            Duration = offering.Duration,
            CallToAction = offering.CallToAction,
            CallToActionUrl = offering.CallToActionUrl,
            Tags = offering.Tags.Select(tag => tag.Name).ToList(),
            SortOrder = offering.SortOrder,
            IsFeatured = offering.IsFeatured,
            IsPublished = offering.IsPublished,
            SeoTitle = offering.SeoTitle,
            SeoDescription = offering.SeoDescription,
            CreatedAt = offering.CreatedAt,
            UpdatedAt = offering.UpdatedAt
        };
    }
}

