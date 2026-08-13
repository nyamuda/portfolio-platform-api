using PortfolioPlatform.Api.Enums.Experiences;

namespace PortfolioPlatform.Api.Dtos.ExperienceTypes;

/// <summary>
/// Search, sorting, and pagination values for experience type list endpoints.
/// </summary>
public class ExperienceTypeQueryParams
{
    /// <summary>
    /// Optional search text used to match the type name or description.
    /// </summary>
    public string? Search { get; set; }

    /// <summary>
    /// Sort option used for the returned type list.
    /// </summary>
    public ExperienceTypeSortOption SortBy { get; set; } = ExperienceTypeSortOption.Manual;

    /// <summary>
    /// One-based page number requested by the type list paginator.
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// Number of types requested per page.
    /// </summary>
    public int PageSize { get; set; } = 20;
}
