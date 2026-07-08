using PortfolioPlatform.Api.Enums.Projects;

namespace PortfolioPlatform.Api.Dtos.Projects;

/// <summary>
/// Filter values accepted by project list endpoints.
/// </summary>
public class ProjectFilters
{
    /// <summary>
    /// Optional text used to match project title, summary, content text, or tags.
    /// </summary>
    public string? SearchTerm { get; set; }

    /// <summary>
    /// Publication state to include in the result set.
    /// </summary>
    public ProjectStatus Status { get; set; } = ProjectStatus.All;

    /// <summary>
    /// Featured-state filter to apply to the result set.
    /// </summary>
    public FeaturedFilter Featured { get; set; } = FeaturedFilter.All;

    /// <summary>
    /// Sort order used for the returned project list.
    /// </summary>
    public SortOption SortBy { get; set; } = SortOption.Recent;

    /// <summary>
    /// One-based page number requested by the project list paginator.
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// Number of projects requested per page. The current endpoint still returns a list,
    /// but keeping this here makes the contract ready for pagination.
    /// </summary>
    public int PageSize { get; set; } = 12;
}





