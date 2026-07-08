namespace PortfolioPlatform.Api.Enums.Projects;

/// <summary>
/// Publication state filters supported by owner project list endpoints.
/// </summary>
public enum ProjectStatus
{
    /// <summary>
    /// Return published and draft projects.
    /// </summary>
    All,

    /// <summary>
    /// Return only projects visible on the public profile.
    /// </summary>
    Published,

    /// <summary>
    /// Return only projects still being edited privately.
    /// </summary>
    Draft
}
