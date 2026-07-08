namespace PortfolioPlatform.Api.Models;

/// <summary>
/// Represents one page of results plus the metadata the frontend needs to render pagination controls.
/// </summary>
/// <typeparam name="T">The DTO type contained in the page.</typeparam>
public class PageInfo<T>
{
    /// <summary>
    /// The current page number using a one-based index.
    /// </summary>
    public required int Page { get; set; }

    /// <summary>
    /// The number of items requested for each page.
    /// </summary>
    public required int PageSize { get; set; }

    /// <summary>
    /// Indicates whether another page exists after the current page.
    /// </summary>
    public required bool HasMore { get; set; }

    /// <summary>
    /// The total number of matching items before pagination is applied.
    /// </summary>
    public int? TotalItems { get; set; }

    /// <summary>
    /// Items returned for the current page.
    /// </summary>
    public required List<T> Items { get; set; }
}
