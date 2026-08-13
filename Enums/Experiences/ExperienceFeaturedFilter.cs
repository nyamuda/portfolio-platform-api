namespace PortfolioPlatform.Api.Enums.Experiences;

/// <summary>
/// Featured-state filter used by owner experience list endpoints.
/// </summary>
public enum ExperienceFeaturedFilter
{
    /// <summary>
    /// Include featured and regular experiences.
    /// </summary>
    All,

    /// <summary>
    /// Include only highlighted experiences.
    /// </summary>
    Featured,

    /// <summary>
    /// Include only experiences that are not highlighted.
    /// </summary>
    Regular
}
