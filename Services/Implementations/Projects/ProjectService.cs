using Microsoft.EntityFrameworkCore;
using PortfolioPlatform.Api.Data;
using PortfolioPlatform.Api.Dtos.Projects;
using PortfolioPlatform.Api.Exceptions;
using PortfolioPlatform.Api.Models.Content;
using PortfolioPlatform.Api.Services.Abstractions.Projects;

namespace PortfolioPlatform.Api.Services.Implementations.Projects;

/// <summary>
/// Handles project management for profile owners and public visitors.
/// </summary>
public class ProjectService(ApplicationDbContext context) : IProjectService
{
    private readonly ApplicationDbContext _context = context;

    /// <inheritdoc/>
    public async Task<List<ProjectDto>> GetMineAsync(int userId)
    {
        // Projects are owned through a profile. This lookup gives us the profile id and proves ownership.
        int profileId = await GetOwnedProfileIdAsync(userId);

        // Owner reads include drafts because the project editor and dashboard need unfinished work.
        // Public reads have their own stricter filters, so drafts are not exposed to visitors.
        return await ProjectDtos(
                _context
                    .Projects
                    .AsNoTracking()
                    .Where(project => project.ProfileId == profileId)
            )
            // Featured projects should appear first because they are the work the owner wants to highlight.
            .OrderByDescending(project => project.IsFeatured)
            // SortOrder gives the owner manual control when several projects are featured or grouped.
            .ThenBy(project => project.SortOrder)
            // Title ordering gives a stable fallback when featured and manual sort values are the same.
            .ThenBy(project => project.Title)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<ProjectDto> GetMineByIdAsync(int userId, int projectId)
    {
        // Resolve the profile first so the project query is always scoped to the authenticated owner.
        int profileId = await GetOwnedProfileIdAsync(userId);

        // Keep this projected even for detail reads. We do not need to load Profile/User objects here.
        return await ProjectDtos(
                _context
                    .Projects
                    .AsNoTracking()
                    .Where(project => project.Id == projectId && project.ProfileId == profileId)
            )
            .FirstOrDefaultAsync()
            ?? throw new KeyNotFoundException($"Project with ID '{projectId}' was not found.");
    }

    /// <inheritdoc/>
    public async Task<List<ProjectDto>> GetPublicByProfileSlugAsync(string profileSlug)
    {
        // Public project lists are intentionally strict: the profile must be published and the
        // individual project must be published. This lets owners draft privately before launch.
        return await ProjectDtos(
                _context
                    .Projects
                    .AsNoTracking()
                    .Where(
                        project =>
                            project.IsPublished
                            && project.Profile.IsPublished
                            && project.Profile.Slug == profileSlug
                    )
            )
            // Public ordering mirrors the owner ordering so the profile appears exactly as intended.
            .OrderByDescending(project => project.IsFeatured)
            .ThenBy(project => project.SortOrder)
            .ThenBy(project => project.Title)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<ProjectDto> GetPublicBySlugAsync(string profileSlug, string projectSlug)
    {
        // Project slugs are unique within a profile, not globally. The profile slug is therefore
        // part of the lookup so two people can use the same project slug on different profiles.
        return await ProjectDtos(
                _context
                    .Projects
                    .AsNoTracking()
                    .Where(
                        project =>
                            project.Slug == projectSlug
                            && project.IsPublished
                            && project.Profile.IsPublished
                            && project.Profile.Slug == profileSlug
                    )
            )
            .FirstOrDefaultAsync()
            ?? throw new KeyNotFoundException($"Project with slug '{projectSlug}' was not found.");
    }

    /// <inheritdoc/>
    public async Task<ProjectDto> CreateAsync(int userId, UpsertProjectDto dto)
    {
        // A project needs a profile because public project URLs sit under the profile route.
        int profileId = await GetOwnedProfileIdAsync(userId);

        // Slugs become public URLs, so reject duplicates before creating the record.
        await EnsureSlugIsAvailableAsync(profileId, dto.Slug);

        // Set the required identity fields first. ApplyChanges fills the editable content fields below.
        Project project = new()
        {
            ProfileId = profileId,
            Title = dto.Title,
            Slug = dto.Slug,
            Summary = dto.Summary,
            CreatedAt = DateTime.UtcNow
        };

        // Use the same assignment helper as update so create/update behavior stays aligned.
        ApplyChanges(project, dto);

        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        // Reload through the read path so the response shape matches every other owner detail response.
        return await GetMineByIdAsync(userId, project.Id);
    }

    /// <inheritdoc/>
    public async Task<ProjectDto> UpdateAsync(int userId, int projectId, UpsertProjectDto dto)
    {
        // Resolve ownership first. The update query is scoped by ProfileId so one user cannot edit another user's project.
        int profileId = await GetOwnedProfileIdAsync(userId);

        Project project = await _context
            .Projects
            .FirstOrDefaultAsync(project => project.Id == projectId && project.ProfileId == profileId)
            ?? throw new KeyNotFoundException($"Project with ID '{projectId}' was not found.");

        // Exclude the current project so a user can save changes without changing the slug.
        await EnsureSlugIsAvailableAsync(profileId, dto.Slug, projectId);

        // Keep all editable fields in one helper so new fields do not get missed in create or update.
        ApplyChanges(project, dto);
        project.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Return the projected DTO rather than the tracked entity.
        return await GetMineByIdAsync(userId, project.Id);
    }

    /// <inheritdoc/>
    public async Task DeleteAsync(int userId, int projectId)
    {
        // Deletes must be scoped just as tightly as updates. Resolve the owner profile first.
        int profileId = await GetOwnedProfileIdAsync(userId);

        Project project = await _context
            .Projects
            .FirstOrDefaultAsync(project => project.Id == projectId && project.ProfileId == profileId)
            ?? throw new KeyNotFoundException($"Project with ID '{projectId}' was not found.");

        _context.Projects.Remove(project);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Converts a project query into the API response shape without loading full entity graphs.
    /// </summary>
    /// <param name="query">The project query after ownership or public visibility filters have already been applied.</param>
    /// <returns>A projected query that returns project DTOs.</returns>
    private static IQueryable<ProjectDto> ProjectDtos(IQueryable<Project> query)
    {
        // This method centralizes the projection so all project endpoints return the same shape.
        // It also prevents accidental over-fetching as the model grows over time.
        return query.Select(project => new ProjectDto
        {
            Id = project.Id,
            ProfileId = project.ProfileId,
            Title = project.Title,
            Slug = project.Slug,
            Summary = project.Summary,
            Problem = project.Problem,
            Solution = project.Solution,

            // The frontend owns the rich editor and sends both HTML and plain text.
            // HTML is used for display; plain text is useful for previews/search later.
            ContentHtml = project.ContentHtml,
            ContentText = project.ContentText,

            TechStack = project.TechStack,

            // The API stores only URLs for media. The actual uploads are handled by the frontend.
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
        });
    }

    /// <summary>
    /// Finds the profile owned by the authenticated user and returns its id for scoped content operations.
    /// </summary>
    /// <param name="userId">The authenticated user's identifier.</param>
    /// <returns>The profile id owned by the authenticated user.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the user has not created a profile yet.</exception>
    private async Task<int> GetOwnedProfileIdAsync(int userId)
    {
        // Projects are attached to a profile. If a user has not created a profile yet,
        // there is nowhere sensible to display a project publicly.
        int? profileId = await _context
            .Profiles
            .AsNoTracking()
            .Where(profile => profile.UserId == userId)
            .Select(profile => (int?)profile.Id)
            .FirstOrDefaultAsync();

        // The exception message is written for the API caller/frontend, not for database debugging.
        return profileId ?? throw new InvalidOperationException("Create your profile before adding projects.");
    }

    /// <summary>
    /// Checks whether a project slug can be used inside a specific profile.
    /// </summary>
    /// <param name="profileId">The profile that owns the project.</param>
    /// <param name="slug">The URL-safe slug the user wants to save.</param>
    /// <param name="currentProjectId">The existing project id to ignore when updating a project.</param>
    /// <exception cref="ConflictException">Thrown when another project on the same profile already uses the slug.</exception>
    private async Task EnsureSlugIsAvailableAsync(int profileId, string slug, int? currentProjectId = null)
    {
        // A slug only needs to be unique inside one profile. This keeps URLs friendly without forcing
        // every creator on the platform to avoid project titles that another creator has already used.
        bool slugAlreadyInUse = await _context
            .Projects
            .AnyAsync(
                project =>
                    project.ProfileId == profileId
                    && project.Slug == slug
                    && (currentProjectId == null || project.Id != currentProjectId.Value)
            );

        if (slugAlreadyInUse)
            throw new ConflictException("This project link is already used on your profile.");
    }

    /// <summary>
    /// Copies editable fields from the incoming DTO onto a project entity.
    /// </summary>
    /// <param name="project">The project entity being created or updated.</param>
    /// <param name="dto">The validated request data from the caller.</param>
    private static void ApplyChanges(Project project, UpsertProjectDto dto)
    {
        // Keep all assignable fields here. It makes create and update easier to audit,
        // especially when new editor fields are added later.
        project.Title = dto.Title;
        project.Slug = dto.Slug;
        project.Summary = dto.Summary;

        // These fields support a case-study style project page: what was wrong, and what was built.
        project.Problem = dto.Problem;
        project.Solution = dto.Solution;

        // The frontend owns the rich editor and sends both HTML and plain text.
        project.ContentHtml = dto.ContentHtml;
        project.ContentText = dto.ContentText;

        // Tech stack is intentionally a simple string list so it works for developers, tutors,
        // creators, and anyone else describing tools or skills used in the work.
        project.TechStack = dto.TechStack;

        // The API stores only the final URLs. Actual file uploads are handled by the frontend.
        project.CoverImageUrl = dto.CoverImageUrl;
        project.ScreenshotUrls = dto.ScreenshotUrls;

        // External links are optional because not every project has a live demo or repository.
        project.ProjectUrl = dto.ProjectUrl;
        project.RepositoryUrl = dto.RepositoryUrl;

        // These fields control how the project appears on the public profile.
        project.SortOrder = dto.SortOrder;
        project.IsFeatured = dto.IsFeatured;
        project.IsPublished = dto.IsPublished;

        // SEO fields are optional overrides for public pages.
        project.SeoTitle = dto.SeoTitle;
        project.SeoDescription = dto.SeoDescription;
    }
}
