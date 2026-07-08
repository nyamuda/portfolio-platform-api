namespace PortfolioPlatform.Api.Enums.BlogPosts;

/// <summary>
/// Featured-state filter used by owner-facing blog post lists.
/// </summary>
public enum BlogPostFeaturedFilter
{
    /// <summary>Include featured and regular posts.</summary>
    All,

    /// <summary>Include only posts highlighted by the owner.</summary>
    Featured,

    /// <summary>Include only regular, non-featured posts.</summary>
    Regular
}
