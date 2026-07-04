using PortfolioPlatform.Api.Models.Content;

namespace PortfolioPlatform.Api.Dtos.Projects;

/// <summary>
/// Project details returned by owner and public project endpoints.
/// </summary>
public class ProjectDto
{
    /// <summary>
    /// The unique database identifier for the project.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The profile that owns and displays this project.
    /// </summary>
    public int ProfileId { get; set; }

    /// <summary>
    /// The public title shown on project cards and the project details page.
    /// </summary>
    public required string Title { get; set; }

    /// <summary>
    /// The URL-safe project slug used inside the owning profile's public route.
    /// </summary>
    public required string Slug { get; set; }

    /// <summary>
    /// A short description of what the project is and why it matters.
    /// </summary>
    public required string Summary { get; set; }

    /// <summary>
    /// Optional problem statement that explains the need behind the project.
    /// </summary>
    public string? Problem { get; set; }

    /// <summary>
    /// Optional solution summary that explains how the project addresses the problem.
    /// </summary>
    public string? Solution { get; set; }

    /// <summary>
    /// Rich HTML content for the full case study or project write-up.
    /// </summary>
    public string? ContentHtml { get; set; }

    /// <summary>
    /// Plain-text version of the write-up for search, previews, and fallbacks.
    /// </summary>
    public string? ContentText { get; set; }

    /// <summary>
    /// Technologies, tools, skills, or subject tags connected to the project.
    /// </summary>
    public List<string> TechStack { get; set; } = [];

    /// <summary>
    /// Public URL for the main cover image uploaded by the frontend.
    /// </summary>
    public string? CoverImageUrl { get; set; }

    /// <summary>
    /// Public URLs for supporting screenshots uploaded by the frontend.
    /// </summary>
    public List<string> ScreenshotUrls { get; set; } = [];

    /// <summary>
    /// Optional link to the live project, product, lesson, resource, or demo.
    /// </summary>
    public string? ProjectUrl { get; set; }

    /// <summary>
    /// Optional repository link when the project has source code to share.
    /// </summary>
    public string? RepositoryUrl { get; set; }

    /// <summary>
    /// Manual ordering value used to arrange projects on a profile.
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// Whether this project should be highlighted ahead of regular projects.
    /// </summary>
    public bool IsFeatured { get; set; }

    /// <summary>
    /// Whether this project is visible on the public profile.
    /// </summary>
    public bool IsPublished { get; set; }

    /// <summary>
    /// Optional browser/search title for the public project page.
    /// </summary>
    public string? SeoTitle { get; set; }

    /// <summary>
    /// Optional browser/search description for the public project page.
    /// </summary>
    public string? SeoDescription { get; set; }

    /// <summary>
    /// When the project was first created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// When the project was last updated, if it has been edited after creation.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Maps a project entity to a DTO.
    /// </summary>
    public static ProjectDto MapFrom(Project project)
    {
        return new ProjectDto
        {
            Id = project.Id,
            ProfileId = project.ProfileId,
            Title = project.Title,
            Slug = project.Slug,
            Summary = project.Summary,
            Problem = project.Problem,
            Solution = project.Solution,
            ContentHtml = project.ContentHtml,
            ContentText = project.ContentText,
            TechStack = project.TechStack,
            CoverImageUrl = project.CoverImageUrl,
            ScreenshotUrls = project.ScreenshotUrls,
            ProjectUrl = project.ProjectUrl,
            RepositoryUrl = project.RepositoryUrl,
            SortOrder = project.SortOrder,
            IsFeatured = project.IsFeatured,
            IsPublished = project.IsPublished,
            SeoTitle = project.SeoTitle,
            SeoDescription = project.SeoDescription,
            CreatedAt = project.CreatedAt,
            UpdatedAt = project.UpdatedAt
        };
    }
}
