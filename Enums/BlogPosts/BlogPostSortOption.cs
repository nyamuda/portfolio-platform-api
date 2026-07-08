namespace PortfolioPlatform.Api.Enums.BlogPosts;

/// <summary>
/// Sort options supported by owner-facing blog post lists.
/// </summary>
public enum BlogPostSortOption
{
    /// <summary>Show recently updated or created posts first.</summary>
    Recent,

    /// <summary>Show older posts first.</summary>
    Oldest,

    /// <summary>Sort alphabetically by post title.</summary>
    Title,

    /// <summary>Use featured state and manual sort order.</summary>
    Manual
}
