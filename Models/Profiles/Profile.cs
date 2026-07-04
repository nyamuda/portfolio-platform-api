using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using PortfolioPlatform.Api.Models.Content;
using PortfolioPlatform.Api.Models.Users;

namespace PortfolioPlatform.Api.Models.Profiles;

/// <summary>
/// Public-facing profile owned by a user account.
/// </summary>
/// <remarks>
/// The profile is the public home for a person's work. It owns projects and blog posts, while the
/// <see cref="User"/> model stays focused on authentication and account ownership.
/// </remarks>
[Index(nameof(UserId), IsUnique = true)]
[Index(nameof(Slug), IsUnique = true)]
public class Profile
{
    /// <summary>
    /// Internal primary key for the profile.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Foreign key of the account that owns this profile.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Account that owns and manages this profile.
    /// </summary>
    public User User { get; set; } = null!;

    /// <summary>
    /// Name shown prominently at the top of the public profile.
    /// </summary>
    [StringLength(160)]
    public required string DisplayName { get; set; }

    /// <summary>
    /// Unique URL-friendly identifier used for public profile routes.
    /// </summary>
    /// <remarks>
    /// This slug is globally unique because it identifies the public profile itself.
    /// </remarks>
    [StringLength(120)]
    public required string Slug { get; set; }

    /// <summary>
    /// Short headline that tells visitors who the person is or what they do.
    /// </summary>
    [StringLength(220)]
    public required string Headline { get; set; }

    /// <summary>
    /// Longer public introduction or biography.
    /// </summary>
    /// <remarks>
    /// This should explain the person's background, skills, work, or teaching focus in a human way.
    /// </remarks>
    [StringLength(4000)]
    public string? Bio { get; set; }

    /// <summary>
    /// Optional short statement about what the person is currently focused on.
    /// </summary>
    [StringLength(300)]
    public string? Focus { get; set; }

    /// <summary>
    /// Public URL of the profile avatar image uploaded by the frontend.
    /// </summary>
    /// <remarks>
    /// The API stores only the final URL. File upload and storage are handled by the frontend.
    /// </remarks>
    public string? AvatarUrl { get; set; }

    /// <summary>
    /// Public URL of the cover image shown on public profile pages.
    /// </summary>
    /// <remarks>
    /// The API stores only the final URL. File upload and storage are handled by the frontend.
    /// </remarks>
    public string? CoverImageUrl { get; set; }

    /// <summary>
    /// Optional location shown on the public profile.
    /// </summary>
    [StringLength(160)]
    public string? Location { get; set; }

    /// <summary>
    /// Optional personal, business, or portfolio website URL.
    /// </summary>
    [StringLength(500)]
    public string? WebsiteUrl { get; set; }

    /// <summary>
    /// Optional GitHub profile URL.
    /// </summary>
    [StringLength(500)]
    public string? GitHubUrl { get; set; }

    /// <summary>
    /// Optional LinkedIn profile URL.
    /// </summary>
    [StringLength(500)]
    public string? LinkedInUrl { get; set; }

    /// <summary>
    /// Optional X/Twitter profile URL.
    /// </summary>
    [StringLength(500)]
    public string? XUrl { get; set; }

    /// <summary>
    /// Optional SEO title override for the public profile page.
    /// </summary>
    [StringLength(180)]
    public string? SeoTitle { get; set; }

    /// <summary>
    /// Optional SEO description override for the public profile page.
    /// </summary>
    [StringLength(320)]
    public string? SeoDescription { get; set; }

    /// <summary>
    /// Indicates whether the profile is visible to public visitors.
    /// </summary>
    /// <remarks>
    /// Draft profiles remain available to the owner but are hidden from public endpoints.
    /// </remarks>
    public bool IsPublished { get; set; }

    /// <summary>
    /// Date and time when the profile was created in UTC.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date and time when the profile was last updated in UTC.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Projects created under this profile.
    /// </summary>
    /// <remarks>
    /// The collection contains drafts and published projects for owner views. Public services filter it.
    /// </remarks>
    public List<Project> Projects { get; set; } = [];

    /// <summary>
    /// Blog posts created under this profile.
    /// </summary>
    /// <remarks>
    /// The collection contains drafts and published posts for owner views. Public services filter it.
    /// </remarks>
    public List<BlogPost> BlogPosts { get; set; } = [];
}
