namespace PortfolioPlatform.Api.Enums.BlogPosts;

/// <summary>
/// Publication-state filter used by owner-facing blog post lists.
/// </summary>
public enum BlogPostStatus
{
    /// <summary>Include both published posts and drafts.</summary>
    All,

    /// <summary>Include only posts visible on the public profile.</summary>
    Published,

    /// <summary>Include only posts that are still private drafts.</summary>
    Draft
}
