namespace PortfolioPlatform.Api.Enums.Tags;

/// <summary>
/// Defines the supported sorting options when browsing tags.
/// </summary>
public enum TagSortOption
{
    /// <summary>
    /// Sort tags by how often they are used across public content.
    /// </summary>
    Popularity,

    /// <summary>
    /// Sort tags alphabetically by name.
    /// </summary>
    Name,

    /// <summary>
    /// Sort tags by newest first.
    /// </summary>
    New
}
