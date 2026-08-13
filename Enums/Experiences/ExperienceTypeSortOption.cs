namespace PortfolioPlatform.Api.Enums.Experiences;

/// <summary>
/// Sort options used by experience type management endpoints.
/// </summary>
public enum ExperienceTypeSortOption
{
    /// <summary>
    /// Sort alphabetically by type name.
    /// </summary>
    Name,

    /// <summary>
    /// Sort by newest types first.
    /// </summary>
    New,

    /// <summary>
    /// Sort by number of linked experiences first.
    /// </summary>
    Popularity,

    /// <summary>
    /// Sort by featured flag and manual ordering.
    /// </summary>
    Manual
}
