using PortfolioPlatform.Api.Enums.Tags;

namespace PortfolioPlatform.Api.Models.Content;

/// <summary>
/// Represents the query parameters supported when fetching portfolio tags.
/// </summary>
public class TagQueryParams
{
    /// <summary>
    /// The page number to retrieve. Page numbers are one-based.
    /// </summary>
    public required int Page { get; set; } = 1;

    /// <summary>
    /// The number of tags to return on each page.
    /// </summary>
    public required int PageSize { get; set; } = 20;

    /// <summary>
    /// Sorting option used when returning tags.
    /// </summary>
    public TagSortOption SortBy { get; set; } = TagSortOption.Popularity;

    /// <summary>
    /// Optional search text used to filter tags by name.
    /// </summary>
    public string? Search { get; set; } = null;
}
