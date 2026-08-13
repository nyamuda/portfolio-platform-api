namespace PortfolioPlatform.Api.Enums.Experiences;

/// <summary>
/// Sort options used by experience list endpoints.
/// </summary>
public enum ExperienceSortOption
{
    /// <summary>
    /// Show the most recently updated experiences first.
    /// </summary>
    Recent,

    /// <summary>
    /// Show the oldest experiences first.
    /// </summary>
    Oldest,

    /// <summary>
    /// Sort alphabetically by title.
    /// </summary>
    Title,

    /// <summary>
    /// Sort by timeline dates with current and newest experiences first.
    /// </summary>
    Timeline,

    /// <summary>
    /// Sort by featured flag and manual ordering.
    /// </summary>
    Manual
}
