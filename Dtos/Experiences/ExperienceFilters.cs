using PortfolioPlatform.Api.Enums.Experiences;

namespace PortfolioPlatform.Api.Dtos.Experiences;

/// <summary>
/// Filter values accepted by experience list endpoints.
/// </summary>
public class ExperienceFilters
{
    /// <summary>
    /// Optional text used to match experience title, organisation, summary, description, location, type, or tags.
    /// </summary>
    public string? SearchTerm { get; set; }

    /// <summary>
    /// Optional primary type filter.
    /// </summary>
    public int? ExperienceTypeId { get; set; }

    /// <summary>
    /// Publication state to include in the result set.
    /// </summary>
    public ExperienceStatus Status { get; set; } = ExperienceStatus.All;

    /// <summary>
    /// Featured-state filter to apply to the result set.
    /// </summary>
    public ExperienceFeaturedFilter Featured { get; set; } = ExperienceFeaturedFilter.All;

    /// <summary>
    /// Sort order used for the returned experience list.
    /// </summary>
    public ExperienceSortOption SortBy { get; set; } = ExperienceSortOption.Timeline;

    /// <summary>
    /// One-based page number requested by the experience list paginator.
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// Number of experiences requested per page.
    /// </summary>
    public int PageSize { get; set; } = 12;
}
