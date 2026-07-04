using PortfolioPlatform.Api.Models.Profiles;

namespace PortfolioPlatform.Api.Dtos.Profiles;

/// <summary>
/// Public and owner-facing profile details returned by the API.
/// </summary>
public class ProfileDto
{
    /// <summary>
    /// Unique profile id.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Account id that owns the profile.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Name shown publicly.
    /// </summary>
    public required string DisplayName { get; set; }

    /// <summary>
    /// Public profile slug.
    /// </summary>
    public required string Slug { get; set; }

    /// <summary>
    /// Short headline shown near the display name.
    /// </summary>
    public required string Headline { get; set; }

    /// <summary>
    /// Longer profile biography.
    /// </summary>
    public string? Bio { get; set; }

    /// <summary>
    /// Optional current focus text.
    /// </summary>
    public string? Focus { get; set; }

    /// <summary>
    /// Avatar image URL.
    /// </summary>
    public string? AvatarUrl { get; set; }

    /// <summary>
    /// Cover image URL.
    /// </summary>
    public string? CoverImageUrl { get; set; }

    /// <summary>
    /// Optional location text.
    /// </summary>
    public string? Location { get; set; }

    /// <summary>
    /// Optional website URL.
    /// </summary>
    public string? WebsiteUrl { get; set; }

    /// <summary>
    /// Optional GitHub profile URL.
    /// </summary>
    public string? GitHubUrl { get; set; }

    /// <summary>
    /// Optional LinkedIn profile URL.
    /// </summary>
    public string? LinkedInUrl { get; set; }

    /// <summary>
    /// Optional X/Twitter profile URL.
    /// </summary>
    public string? XUrl { get; set; }

    /// <summary>
    /// Optional SEO title.
    /// </summary>
    public string? SeoTitle { get; set; }

    /// <summary>
    /// Optional SEO description.
    /// </summary>
    public string? SeoDescription { get; set; }

    /// <summary>
    /// Whether the profile is public.
    /// </summary>
    public bool IsPublished { get; set; }

    /// <summary>
    /// Number of published projects connected to the profile.
    /// </summary>
    public int PublishedProjectCount { get; set; }

    /// <summary>
    /// Date and time when the profile was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Date and time when the profile was last updated.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Maps a profile entity to a DTO.
    /// </summary>
    public static ProfileDto MapFrom(Profile profile)
    {
        return new ProfileDto
        {
            Id = profile.Id,
            UserId = profile.UserId,
            DisplayName = profile.DisplayName,
            Slug = profile.Slug,
            Headline = profile.Headline,
            Bio = profile.Bio,
            Focus = profile.Focus,
            AvatarUrl = profile.AvatarUrl,
            CoverImageUrl = profile.CoverImageUrl,
            Location = profile.Location,
            WebsiteUrl = profile.WebsiteUrl,
            GitHubUrl = profile.GitHubUrl,
            LinkedInUrl = profile.LinkedInUrl,
            XUrl = profile.XUrl,
            SeoTitle = profile.SeoTitle,
            SeoDescription = profile.SeoDescription,
            IsPublished = profile.IsPublished,
            PublishedProjectCount = profile.Projects.Count(project => project.IsPublished),
            CreatedAt = profile.CreatedAt,
            UpdatedAt = profile.UpdatedAt
        };
    }
}
