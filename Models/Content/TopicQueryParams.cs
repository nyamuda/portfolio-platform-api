using PortfolioPlatform.Api.Enums.Topics;

namespace PortfolioPlatform.Api.Models.Content;

/// <summary>
/// Query values used when browsing managed blog topics.
/// </summary>
public class TopicQueryParams
{
    /// <summary>
    /// Optional text used to search topic names and descriptions.
    /// </summary>
    public string? Search { get; set; }

    /// <summary>
    /// One-based page number requested by the frontend.
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// Number of topics requested per page.
    /// </summary>
    public int PageSize { get; set; } = 10;

    /// <summary>
    /// Sort option selected by the frontend.
    /// </summary>
    public TopicSortOption SortBy { get; set; } = TopicSortOption.Popularity;
}
