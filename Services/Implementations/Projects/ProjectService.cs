using Microsoft.EntityFrameworkCore;
using PortfolioPlatform.Api.Data;
using PortfolioPlatform.Api.Dtos.Projects;
using PortfolioPlatform.Api.Enums.Projects;
using PortfolioPlatform.Api.Exceptions;
using PortfolioPlatform.Api.Models;
using PortfolioPlatform.Api.Models.Content;
using PortfolioPlatform.Api.Services.Abstractions.Projects;
using PortfolioPlatform.Api.Services.Abstractions.Tags;

namespace PortfolioPlatform.Api.Services.Implementations.Projects;

/// <summary>
/// Handles project management for profile owners and public visitors.
/// </summary>
public class ProjectService(ApplicationDbContext context, ITagService tagService) : IProjectService
{
    private readonly ApplicationDbContext _context = context;
    private readonly ITagService _tagService = tagService;

    /// <inheritdoc/>
    public async Task<PageInfo<ProjectDto>> GetMineAsync(int userId, ProjectFilters filters)
    {
        // Projects are owned through a profile. This lookup gives us the profile id and proves ownership.
        int profileId = await GetOwnedProfileIdAsync(userId);

        // Keep incoming paging values sensible. This protects the endpoint from accidental huge requests
        // while still letting the frontend offer a few practical page sizes.
        int page = Math.Max(filters.Page, 1);
        int pageSize = Math.Clamp(filters.PageSize, 1, 50);

        // Owner reads include drafts because the project editor and dashboard need unfinished work.
        // Public reads have their own stricter filters, so drafts are not exposed to visitors.
        IQueryable<Project> query = _context
            .Projects
            .AsNoTracking()
            .Where(project => project.ProfileId == profileId);

        // Keep filtering before counting so TotalItems reflects the current search/filter selection.
        query = ApplyProjectFilters(query, filters);
        int totalItems = await query.CountAsync();

        // Sorting also happens before projection so the database handles ordering efficiently.
        query = ApplyProjectSort(query, filters.SortBy);

        // Apply paging after filtering and sorting. The frontend paginator is 0-based, but the API
        // contract remains 1-based like the rest of the platform.
        List<ProjectDto> projects = await ProjectDtos(query)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PageInfo<ProjectDto>
        {
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            HasMore = totalItems > page * pageSize,
            Items = projects
        };
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

        // Attach tags before the first save, matching the question/tag flow used elsewhere.
        await UpdateProjectTagsAsync(project, dto.Tags);

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
            .Include(project => project.Tags)
            .FirstOrDefaultAsync(project => project.Id == projectId && project.ProfileId == profileId)
            ?? throw new KeyNotFoundException($"Project with ID '{projectId}' was not found.");

        // Exclude the current project so a user can save changes without changing the slug.
        await EnsureSlugIsAvailableAsync(profileId, dto.Slug, projectId);

        // Keep all editable fields in one helper so new fields do not get missed in create or update.
        ApplyChanges(project, dto);

        // Keep the tag collection in sync with the names submitted by the form.
        await UpdateProjectTagsAsync(project, dto.Tags);

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
            .Include(project => project.Tags)
            .FirstOrDefaultAsync(project => project.Id == projectId && project.ProfileId == profileId)
            ?? throw new KeyNotFoundException($"Project with ID '{projectId}' was not found.");

        _context.Projects.Remove(project);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Applies owner-list project filters before projection.
    /// </summary>
    /// <param name="query">The project query already scoped to the authenticated owner.</param>
    /// <param name="filters">Filter values supplied from the request query string.</param>
    /// <returns>The filtered project query.</returns>
    private static IQueryable<Project> ApplyProjectFilters(IQueryable<Project> query, ProjectFilters filters)
    {
        // Status is about public visibility. Owners can still see drafts; this just lets them narrow the list.
        query = filters.Status switch
        {
            ProjectStatus.Published => query.Where(project => project.IsPublished),
            ProjectStatus.Draft => query.Where(project => !project.IsPublished),
            _ => query
        };

        // Featured filtering is separate from status because a project can be featured and still be a draft.
        query = filters.Featured switch
        {
            FeaturedFilter.Featured => query.Where(project => project.IsFeatured),
            FeaturedFilter.Regular => query.Where(project => !project.IsFeatured),
            _ => query
        };

        if (!string.IsNullOrWhiteSpace(filters.SearchTerm))
        {
            string searchTerm = filters.SearchTerm.Trim().ToLower();

            // Keep this search broad enough for owner management without making it expensive.
            // Tags are searched too so owners can find work by skill, subject, tool, or theme.
            query = query.Where(
                project =>
                    project.Title.ToLower().Contains(searchTerm)
                    || project.Summary.ToLower().Contains(searchTerm)
                    || (project.Problem != null && project.Problem.ToLower().Contains(searchTerm))
                    || (project.Solution != null && project.Solution.ToLower().Contains(searchTerm))
                    || (project.ContentText != null && project.ContentText.ToLower().Contains(searchTerm))
                    || project.Tags.Any(tag => tag.Name.ToLower().Contains(searchTerm))
            );
        }

        return query;
    }

    /// <summary>
    /// Applies the selected owner-list sort option to a project query.
    /// </summary>
    /// <param name="query">The filtered project query.</param>
    /// <param name="sortOption">The requested sort option.</param>
    /// <returns>The ordered project query.</returns>
    private static IQueryable<Project> ApplyProjectSort(IQueryable<Project> query, SortOption sortOption)
    {
        // Manual order mirrors the public profile order: featured work first, then the owner's SortOrder.
        if (sortOption == SortOption.Manual)
        {
            return query
                .OrderByDescending(project => project.IsFeatured)
                .ThenBy(project => project.SortOrder)
                .ThenBy(project => project.Title);
        }

        // Title sorting is useful when a creator is tidying a larger list of work.
        if (sortOption == SortOption.Title)
        {
            return query.OrderBy(project => project.Title);
        }

        // Date sorting uses UpdatedAt when available, then falls back to CreatedAt for untouched projects.
        return sortOption == SortOption.Oldest
            ? query.OrderBy(project => project.UpdatedAt ?? project.CreatedAt).ThenBy(project => project.Title)
            : query.OrderByDescending(project => project.UpdatedAt ?? project.CreatedAt).ThenBy(project => project.Title);
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

            Tags = project.Tags.Select(tag => tag.Name).ToList(),

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
    /// Updates the tag navigation collection on a project from submitted tag names.
    /// </summary>
    /// <param name="project">The tracked project entity being created or updated.</param>
    /// <param name="newTagNames">The tag names submitted by the form.</param>
    private async Task UpdateProjectTagsAsync(Project project, List<string> newTagNames)
    {
        // Distinct with a case-insensitive comparer prevents duplicates like "Vue" and "vue".
        var cleanedTagNames = newTagNames
            .Where(tagName => !string.IsNullOrWhiteSpace(tagName))
            .Select(tagName => tagName.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        // Remove tags that are no longer present in the submitted list.
        foreach (var tag in project.Tags.ToList())
        {
            if (!cleanedTagNames.Any(tagName => tagName.Equals(tag.Name, StringComparison.OrdinalIgnoreCase)))
            {
                project.Tags.Remove(tag);
            }
        }

        // Add missing tags through the tag service so existing tag rows are reused.
        foreach (string tagName in cleanedTagNames)
        {
            if (project.Tags.Any(tag => tag.Name.Equals(tagName, StringComparison.OrdinalIgnoreCase)))
                continue;

            Tag tag = await _tagService.GetByNameAsync(tagName); // creates if not exists
            project.Tags.Add(tag);
        }
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

        // Tags keep projects flexible: a developer can list tools, while a tutor,
        // designer, writer, or creator can describe subjects, skills, or themes.
        // Tags are attached through ITagService so the same tag rows can be reused across content.

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






