namespace PortfolioPlatform.Api.Dtos.Dashboard;

/// <summary>
/// Account-owned dashboard summary used by the authenticated app dashboard.
/// </summary>
public class DashboardSummaryDto
{
    /// <summary>
    /// Whether the user has created a profile.
    /// </summary>
    public bool HasProfile { get; set; }

    /// <summary>
    /// Whether the profile is publicly visible.
    /// </summary>
    public bool IsProfilePublished { get; set; }

    /// <summary>
    /// Public profile slug when one exists.
    /// </summary>
    public string? ProfileSlug { get; set; }

    /// <summary>
    /// Simple profile completion score from 0 to 100.
    /// </summary>
    public int ProfileCompletionPercent { get; set; }

    /// <summary>
    /// Total projects owned by the profile.
    /// </summary>
    public int TotalProjects { get; set; }

    /// <summary>
    /// Published projects owned by the profile.
    /// </summary>
    public int PublishedProjects { get; set; }

    /// <summary>
    /// Draft projects owned by the profile.
    /// </summary>
    public int DraftProjects { get; set; }

    /// <summary>
    /// Featured projects owned by the profile.
    /// </summary>
    public int FeaturedProjects { get; set; }

    /// <summary>
    /// Total blog posts owned by the profile.
    /// </summary>
    public int TotalBlogPosts { get; set; }

    /// <summary>
    /// Published blog posts owned by the profile.
    /// </summary>
    public int PublishedBlogPosts { get; set; }

    /// <summary>
    /// Draft blog posts owned by the profile.
    /// </summary>
    public int DraftBlogPosts { get; set; }
    /// <summary>
    /// Next useful setup or publishing action for the user.
    /// </summary>
    public required string NextStep { get; set; }
}

