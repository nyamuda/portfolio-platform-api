using PortfolioPlatform.Api.Enums.Offerings;

namespace PortfolioPlatform.Api.Dtos.Offerings;

/// <summary>
/// Filter values accepted by offering list endpoints.
/// </summary>
public class OfferingFilters
{
    /// <summary>
    /// Optional text used to match offering title, summary, content text, pricing, delivery, or tags.
    /// </summary>
    public string? SearchTerm { get; set; }

    /// <summary>
    /// Publication state to include in the result set.
    /// </summary>
    public OfferingStatus Status { get; set; } = OfferingStatus.All;

    /// <summary>
    /// Featured-state filter to apply to the result set.
    /// </summary>
    public OfferingFeaturedFilter Featured { get; set; } = OfferingFeaturedFilter.All;

    /// <summary>
    /// Sort order used for the returned offering list.
    /// </summary>
    public OfferingSortOption SortBy { get; set; } = OfferingSortOption.Recent;

    /// <summary>
    /// One-based page number requested by the offering list paginator.
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// Number of offerings requested per page.
    /// </summary>
    public int PageSize { get; set; } = 12;
}
