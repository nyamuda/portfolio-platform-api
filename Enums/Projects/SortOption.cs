namespace PortfolioPlatform.Api.Enums.Projects;

/// <summary>
/// Sort options supported by project list endpoints.
/// </summary>
public enum SortOption
{
    /// <summary>
    /// Recently updated projects appear first.
    /// </summary>
    Recent,

    /// <summary>
    /// Older projects appear first.
    /// </summary>
    Oldest,

    /// <summary>
    /// Projects are sorted alphabetically by title.
    /// </summary>
    Title,

    /// <summary>
    /// Projects follow the owner's manual sort order.
    /// </summary>
    Manual
}
