namespace PortfolioPlatform.Api.Enums.Topics;

/// <summary>
/// Defines the supported sorting options when browsing blog topics.
/// </summary>
public enum TopicSortOption
{
    /// <summary>
    /// Sort topics by how many blog posts use them.
    /// </summary>
    Popularity,

    /// <summary>
    /// Sort topics alphabetically by name.
    /// </summary>
    Name,

    /// <summary>
    /// Sort topics by newest first.
    /// </summary>
    New
}
