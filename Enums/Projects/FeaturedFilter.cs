namespace PortfolioPlatform.Api.Enums.Projects;

/// <summary>
/// Featured-state filters supported by owner project list endpoints.
/// </summary>
public enum FeaturedFilter
{
    /// <summary>
    /// Return featured and regular projects.
    /// </summary>
    All,

    /// <summary>
    /// Return only projects marked as featured.
    /// </summary>
    Featured,

    /// <summary>
    /// Return projects that are not currently featured.
    /// </summary>
    Regular
}
