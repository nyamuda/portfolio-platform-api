namespace PortfolioPlatform.Api.Enums.Experiences;

/// <summary>
/// Publication-state filter used by owner experience list endpoints.
/// </summary>
public enum ExperienceStatus
{
    /// <summary>
    /// Include published and draft experiences.
    /// </summary>
    All,

    /// <summary>
    /// Include only experiences visible on the public profile.
    /// </summary>
    Published,

    /// <summary>
    /// Include only private draft experiences.
    /// </summary>
    Draft
}
