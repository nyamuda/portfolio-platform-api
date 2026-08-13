using Microsoft.EntityFrameworkCore;
using PortfolioPlatform.Api.Data;
using PortfolioPlatform.Api.Dtos.Experiences;
using PortfolioPlatform.Api.Enums.Experiences;
using PortfolioPlatform.Api.Models;
using PortfolioPlatform.Api.Models.Content;
using PortfolioPlatform.Api.Services.Abstractions.Experiences;
using PortfolioPlatform.Api.Services.Abstractions.Tags;

namespace PortfolioPlatform.Api.Services.Implementations.Experiences;

/// <summary>
/// Handles timeline experience management for profile owners and public visitors.
/// </summary>
public class ExperienceService(ApplicationDbContext context, ITagService tagService)
    : IExperienceService
{
    private readonly ApplicationDbContext _context = context;
    private readonly ITagService _tagService = tagService;

    /// <inheritdoc/>
    public async Task<PageInfo<ExperienceDto>> GetMineAsync(int userId, ExperienceFilters filters)
    {
        // Experiences are owned through a profile. This lookup gives us the profile id and proves ownership.
        int profileId = await GetOwnedProfileIdAsync(userId);

        // Keep paging bounded so the frontend cannot accidentally ask for a huge timeline payload.
        int page = Math.Max(filters.Page, 1);
        int pageSize = Math.Clamp(filters.PageSize, 1, 50);

        // Owner reads include drafts because creators need to manage unfinished timeline entries privately.
        IQueryable<Experience> query = _context
            .Experiences
            .AsNoTracking()
            .Where(experience => experience.ProfileId == profileId);

        // Filter before counting so the paginator reflects the exact list being shown.
        query = ApplyExperienceFilters(query, filters);
        int totalItems = await query.CountAsync();

        // Sorting stays inside the database query so paging remains stable and efficient.
        query = ApplyExperienceSort(query, filters.SortBy);

        List<ExperienceDto> experiences = await ExperienceDtos(query)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PageInfo<ExperienceDto>
        {
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            HasMore = totalItems > page * pageSize,
            Items = experiences
        };
    }

    /// <inheritdoc/>
    public async Task<ExperienceDto> GetMineByIdAsync(int userId, int experienceId)
    {
        // Resolve ownership first so the read cannot leak another profile's private draft entry.
        int profileId = await GetOwnedProfileIdAsync(userId);

        return await ExperienceDtos(
                    _context
                        .Experiences
                        .AsNoTracking()
                        .Where(
                            experience => experience.Id == experienceId && experience.ProfileId == profileId
                        )
                )
                .FirstOrDefaultAsync()
            ?? throw new KeyNotFoundException($"Experience with ID '{experienceId}' was not found.");
    }

    /// <inheritdoc/>
    public async Task<List<ExperienceDto>> GetPublicByProfileSlugAsync(string profileSlug)
    {
        // Public timeline reads only include entries the owner has published on a published profile.
        return await ExperienceDtos(
                _context
                    .Experiences
                    .AsNoTracking()
                    .Where(
                        experience =>
                            experience.IsPublished
                            && experience.Profile.IsPublished
                            && experience.Profile.Slug == profileSlug
                    )
            )
            // Public timelines should feel chronological by default, while still letting featured entries stand out.
            .OrderByDescending(experience => experience.IsFeatured)
            .ThenByDescending(experience => experience.IsCurrent)
            .ThenByDescending(experience => experience.StartDate)
            .ThenBy(experience => experience.SortOrder)
            .ThenBy(experience => experience.Title)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<ExperienceDto> CreateAsync(int userId, UpsertExperienceDto dto)
    {
        // A timeline entry needs a profile because public experiences are shown under the profile route.
        int profileId = await GetOwnedProfileIdAsync(userId);

        // Validate the selected type before creating the row so the frontend gets a clear error.
        await EnsureExperienceTypeExistsAsync(dto.ExperienceTypeId);

        Experience experience = new()
        {
            ProfileId = profileId,
            CreatedAt = DateTime.UtcNow,
            Title = dto.Title
        };

        // Use the same assignment helper as update so create/update behavior stays aligned.
        ApplyChanges(experience, dto);

        // Attach tags before saving so the created response includes the final tag list.
        await UpdateExperienceTagsAsync(experience, dto.Tags);

        _context.Experiences.Add(experience);
        await _context.SaveChangesAsync();

        // Reload through the read path so every response uses the same projection.
        return await GetMineByIdAsync(userId, experience.Id);
    }

    /// <inheritdoc/>
    public async Task<ExperienceDto> UpdateAsync(int userId, int experienceId, UpsertExperienceDto dto)
    {
        // Resolve ownership first. The update query is scoped by ProfileId so users cannot edit each other's timelines.
        int profileId = await GetOwnedProfileIdAsync(userId);

        Experience experience =
            await _context
                .Experiences
                .Include(experience => experience.Tags)
                .FirstOrDefaultAsync(
                    experience => experience.Id == experienceId && experience.ProfileId == profileId
                )
            ?? throw new KeyNotFoundException($"Experience with ID '{experienceId}' was not found.");

        // The selected type is required, so make sure the id still points to managed vocabulary.
        await EnsureExperienceTypeExistsAsync(dto.ExperienceTypeId);

        // Keep all editable fields in one helper so create and update remain easy to compare.
        ApplyChanges(experience, dto);

        // Keep the tag collection in sync with the names submitted by the form.
        await UpdateExperienceTagsAsync(experience, dto.Tags);

        experience.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Return a projected DTO rather than the tracked entity.
        return await GetMineByIdAsync(userId, experience.Id);
    }

    /// <inheritdoc/>
    public async Task DeleteAsync(int userId, int experienceId)
    {
        // Deletes must be scoped just as tightly as updates. Resolve the owner profile first.
        int profileId = await GetOwnedProfileIdAsync(userId);

        Experience experience =
            await _context
                .Experiences
                .Include(experience => experience.Tags)
                .FirstOrDefaultAsync(
                    experience => experience.Id == experienceId && experience.ProfileId == profileId
                )
            ?? throw new KeyNotFoundException($"Experience with ID '{experienceId}' was not found.");

        _context.Experiences.Remove(experience);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Applies owner-list experience filters before projection.
    /// </summary>
    /// <param name="query">The experience query already scoped to the authenticated owner.</param>
    /// <param name="filters">Filter values supplied from the request query string.</param>
    /// <returns>The filtered experience query.</returns>
    private static IQueryable<Experience> ApplyExperienceFilters(
        IQueryable<Experience> query,
        ExperienceFilters filters
    )
    {
        // Status is about public visibility. Owners can still see drafts; this only narrows the list.
        query = filters.Status switch
        {
            ExperienceStatus.Published => query.Where(experience => experience.IsPublished),
            ExperienceStatus.Draft => query.Where(experience => !experience.IsPublished),
            _ => query
        };

        // Featured filtering is separate from status because an entry can be featured and still be a draft.
        query = filters.Featured switch
        {
            ExperienceFeaturedFilter.Featured => query.Where(experience => experience.IsFeatured),
            ExperienceFeaturedFilter.Regular => query.Where(experience => !experience.IsFeatured),
            _ => query
        };

        if (filters.ExperienceTypeId is not null)
        {
            // The type filter lets owners focus on Work, Education, Volunteering, and similar timeline groups.
            query = query.Where(experience => experience.ExperienceTypeId == filters.ExperienceTypeId.Value);
        }

        if (!string.IsNullOrWhiteSpace(filters.SearchTerm))
        {
            string searchTerm = filters.SearchTerm.Trim().ToLower();

            // Search the fields creators naturally remember: title, organisation, location, body text, type, and tags.
            query = query.Where(
                experience =>
                    experience.Title.ToLower().Contains(searchTerm)
                    || (experience.Organization != null && experience.Organization.ToLower().Contains(searchTerm))
                    || (experience.Location != null && experience.Location.ToLower().Contains(searchTerm))
                    || (experience.Summary != null && experience.Summary.ToLower().Contains(searchTerm))
                    || (
                        experience.DescriptionText != null
                        && experience.DescriptionText.ToLower().Contains(searchTerm)
                    )
                    || experience.ExperienceType.Name.ToLower().Contains(searchTerm)
                    || experience.Tags.Any(tag => tag.Name.ToLower().Contains(searchTerm))
            );
        }

        return query;
    }

    /// <summary>
    /// Applies the selected owner-list sort option to an experience query.
    /// </summary>
    /// <param name="query">The filtered experience query.</param>
    /// <param name="sortOption">The requested sort option.</param>
    /// <returns>The ordered experience query.</returns>
    private static IQueryable<Experience> ApplyExperienceSort(
        IQueryable<Experience> query,
        ExperienceSortOption sortOption
    )
    {
        if (sortOption == ExperienceSortOption.Manual)
        {
            return query
                .OrderByDescending(experience => experience.IsFeatured)
                .ThenBy(experience => experience.SortOrder)
                .ThenBy(experience => experience.Title);
        }

        if (sortOption == ExperienceSortOption.Title)
        {
            return query.OrderBy(experience => experience.Title);
        }

        if (sortOption == ExperienceSortOption.Timeline)
        {
            return query
                .OrderByDescending(experience => experience.IsCurrent)
                .ThenByDescending(experience => experience.StartDate)
                .ThenByDescending(experience => experience.EndDate)
                .ThenBy(experience => experience.SortOrder)
                .ThenBy(experience => experience.Title);
        }

        // Date sorting uses UpdatedAt when available, then falls back to CreatedAt for untouched experiences.
        return sortOption == ExperienceSortOption.Oldest
            ? query
                .OrderBy(experience => experience.UpdatedAt ?? experience.CreatedAt)
                .ThenBy(experience => experience.Title)
            : query
                .OrderByDescending(experience => experience.UpdatedAt ?? experience.CreatedAt)
                .ThenBy(experience => experience.Title);
    }

    /// <summary>
    /// Converts an experience query into the API response shape without loading full entity graphs.
    /// </summary>
    /// <param name="query">The experience query after ownership or public visibility filters have already been applied.</param>
    /// <returns>A projected query that returns experience DTOs.</returns>
    private static IQueryable<ExperienceDto> ExperienceDtos(IQueryable<Experience> query)
    {
        // Centralizing the projection keeps every endpoint consistent and prevents accidental over-fetching.
        return query.Select(
            experience =>
                new ExperienceDto
                {
                    Id = experience.Id,
                    ProfileId = experience.ProfileId,
                    ExperienceTypeId = experience.ExperienceTypeId,
                    ExperienceTypeName = experience.ExperienceType.Name,
                    ExperienceTypeSlug = experience.ExperienceType.Slug,
                    ExperienceTypeColorHex = experience.ExperienceType.ColorHex,
                    ExperienceTypeIconName = experience.ExperienceType.IconName,
                    Title = experience.Title,
                    Organization = experience.Organization,
                    Location = experience.Location,
                    StartDate = experience.StartDate,
                    EndDate = experience.EndDate,
                    IsCurrent = experience.IsCurrent,
                    Summary = experience.Summary,
                    DescriptionHtml = experience.DescriptionHtml,
                    DescriptionText = experience.DescriptionText,
                    ExternalUrl = experience.ExternalUrl,
                    Tags = experience.Tags.Select(tag => tag.Name).ToList(),
                    SortOrder = experience.SortOrder,
                    IsFeatured = experience.IsFeatured,
                    IsPublished = experience.IsPublished,
                    CreatedAt = experience.CreatedAt,
                    UpdatedAt = experience.UpdatedAt
                }
        );
    }

    /// <summary>
    /// Finds the profile owned by the authenticated user and returns its id for scoped timeline operations.
    /// </summary>
    /// <param name="userId">The authenticated user's identifier.</param>
    /// <returns>The profile id owned by the authenticated user.</returns>
    private async Task<int> GetOwnedProfileIdAsync(int userId)
    {
        // Experiences are attached to a profile. Without a profile, there is nowhere to show them publicly.
        int? profileId = await _context
            .Profiles
            .AsNoTracking()
            .Where(profile => profile.UserId == userId)
            .Select(profile => (int?)profile.Id)
            .FirstOrDefaultAsync();

        return profileId
            ?? throw new InvalidOperationException("Create your profile before adding experiences.");
    }

    /// <summary>
    /// Checks that the selected experience type exists before it is assigned to an experience.
    /// </summary>
    /// <param name="experienceTypeId">The selected experience type id.</param>
    private async Task EnsureExperienceTypeExistsAsync(int experienceTypeId)
    {
        // A missing type usually means the form used stale data, so return a clear message instead of a database error.
        bool exists = await _context.ExperienceTypes.AnyAsync(type => type.Id == experienceTypeId);

        if (!exists)
            throw new KeyNotFoundException($"Experience type with ID '{experienceTypeId}' was not found.");
    }

    /// <summary>
    /// Updates the tag navigation collection on an experience from submitted tag names.
    /// </summary>
    /// <param name="experience">The tracked experience entity being created or updated.</param>
    /// <param name="newTagNames">The tag names submitted by the form.</param>
    private async Task UpdateExperienceTagsAsync(Experience experience, List<string> newTagNames)
    {
        // Distinct with a case-insensitive comparer prevents duplicates like "Vue" and "vue".
        var cleanedTagNames = newTagNames
            .Where(tagName => !string.IsNullOrWhiteSpace(tagName))
            .Select(tagName => tagName.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        // Remove tags that are no longer present in the submitted list.
        foreach (Tag tag in experience.Tags.ToList())
        {
            if (
                !cleanedTagNames.Any(
                    tagName => tagName.Equals(tag.Name, StringComparison.OrdinalIgnoreCase)
                )
            )
            {
                experience.Tags.Remove(tag);
            }
        }

        // Add missing tags through the tag service so existing tag rows are reused across content types.
        foreach (string tagName in cleanedTagNames)
        {
            if (
                experience
                    .Tags
                    .Any(tag => tag.Name.Equals(tagName, StringComparison.OrdinalIgnoreCase))
            )
                continue;

            Tag tag = await _tagService.GetByNameAsync(tagName);
            experience.Tags.Add(tag);
        }
    }

    /// <summary>
    /// Copies editable fields from the incoming DTO onto an experience entity.
    /// </summary>
    /// <param name="experience">The experience entity being created or updated.</param>
    /// <param name="dto">The validated request data from the caller.</param>
    private static void ApplyChanges(Experience experience, UpsertExperienceDto dto)
    {
        // Keep all assignable fields here. It makes create and update easier to audit as the feature grows.
        experience.ExperienceTypeId = dto.ExperienceTypeId;
        experience.Title = dto.Title;
        experience.Organization = string.IsNullOrWhiteSpace(dto.Organization) ? null : dto.Organization.Trim();
        experience.Location = string.IsNullOrWhiteSpace(dto.Location) ? null : dto.Location.Trim();
        experience.StartDate = dto.StartDate;
        experience.EndDate = dto.IsCurrent ? null : dto.EndDate;
        experience.IsCurrent = dto.IsCurrent;
        experience.Summary = string.IsNullOrWhiteSpace(dto.Summary) ? null : dto.Summary.Trim();

        // The frontend owns the rich editor and sends both HTML and plain text.
        experience.DescriptionHtml = dto.DescriptionHtml;
        experience.DescriptionText = dto.DescriptionText;

        // This optional link points visitors to proof or extra context for the timeline entry.
        experience.ExternalUrl = string.IsNullOrWhiteSpace(dto.ExternalUrl) ? null : dto.ExternalUrl.Trim();

        // These fields control how the experience appears on the public profile.
        experience.SortOrder = dto.SortOrder;
        experience.IsFeatured = dto.IsFeatured;
        experience.IsPublished = dto.IsPublished;
    }
}

