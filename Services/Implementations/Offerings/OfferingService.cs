using Microsoft.EntityFrameworkCore;
using PortfolioPlatform.Api.Data;
using PortfolioPlatform.Api.Dtos.Offerings;
using PortfolioPlatform.Api.Enums.Offerings;
using PortfolioPlatform.Api.Exceptions;
using PortfolioPlatform.Api.Models;
using PortfolioPlatform.Api.Models.Content;
using PortfolioPlatform.Api.Services.Abstractions.Offerings;
using PortfolioPlatform.Api.Services.Abstractions.Tags;

namespace PortfolioPlatform.Api.Services.Implementations.Offerings;

/// <summary>
/// Handles offering management for profile owners and public visitors.
/// </summary>
public class OfferingService(ApplicationDbContext context, ITagService tagService) : IOfferingService
{
    private readonly ApplicationDbContext _context = context;
    private readonly ITagService _tagService = tagService;

    /// <inheritdoc/>
    public async Task<PageInfo<OfferingDto>> GetMineAsync(int userId, OfferingFilters filters)
    {
        // Offerings are owned through a profile. This lookup gives us the profile id and proves ownership.
        int profileId = await GetOwnedProfileIdAsync(userId);

        // Keep incoming paging values sensible so a UI mistake cannot request thousands of rows at once.
        int page = Math.Max(filters.Page, 1);
        int pageSize = Math.Clamp(filters.PageSize, 1, 50);

        // Owner reads include drafts because creators need to manage incomplete offerings privately.
        IQueryable<Offering> query = _context
            .Offerings
            .AsNoTracking()
            .Where(offering => offering.ProfileId == profileId);

        // Filter before counting so the paginator describes the exact result set the owner is viewing.
        query = ApplyOfferingFilters(query, filters);
        int totalItems = await query.CountAsync();

        // Sorting stays in the database query so paging remains stable and efficient.
        query = ApplyOfferingSort(query, filters.SortBy);

        List<OfferingDto> offerings = await OfferingDtos(query)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PageInfo<OfferingDto>
        {
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            HasMore = totalItems > page * pageSize,
            Items = offerings
        };
    }

    /// <inheritdoc/>
    public async Task<OfferingDto> GetMineByIdAsync(int userId, int offeringId)
    {
        // Resolve the profile first so the offering query is always scoped to the authenticated owner.
        int profileId = await GetOwnedProfileIdAsync(userId);

        // Keep detail reads projected. The editor needs the offering fields, not a full entity graph.
        return await OfferingDtos(
                _context
                    .Offerings
                    .AsNoTracking()
                    .Where(offering => offering.Id == offeringId && offering.ProfileId == profileId)
            )
            .FirstOrDefaultAsync()
            ?? throw new KeyNotFoundException($"Offering with ID '{offeringId}' was not found.");
    }

    /// <inheritdoc/>
    public async Task<List<OfferingDto>> GetPublicByProfileSlugAsync(string profileSlug)
    {
        // Public lists only include published offerings on a published profile. Drafts stay private.
        return await OfferingDtos(
                _context
                    .Offerings
                    .AsNoTracking()
                    .Where(
                        offering =>
                            offering.IsPublished
                            && offering.Profile.IsPublished
                            && offering.Profile.Slug == profileSlug
                    )
            )
            // Public ordering mirrors the owner's chosen order, with featured offerings first.
            .OrderByDescending(offering => offering.IsFeatured)
            .ThenBy(offering => offering.SortOrder)
            .ThenBy(offering => offering.Title)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<OfferingDto> GetPublicBySlugAsync(string profileSlug, string offeringSlug)
    {
        // Offering slugs are unique inside a profile, so the profile slug is part of the public lookup.
        return await OfferingDtos(
                _context
                    .Offerings
                    .AsNoTracking()
                    .Where(
                        offering =>
                            offering.Slug == offeringSlug
                            && offering.IsPublished
                            && offering.Profile.IsPublished
                            && offering.Profile.Slug == profileSlug
                    )
            )
            .FirstOrDefaultAsync()
            ?? throw new KeyNotFoundException($"Offering with slug '{offeringSlug}' was not found.");
    }

    /// <inheritdoc/>
    public async Task<OfferingDto> CreateAsync(int userId, UpsertOfferingDto dto)
    {
        // A public offering needs a profile because offering URLs sit under the profile route.
        int profileId = await GetOwnedProfileIdAsync(userId);

        // Slugs become public URLs, so reject duplicates before creating the record.
        await EnsureSlugIsAvailableAsync(profileId, dto.Slug);

        Offering offering = new()
        {
            ProfileId = profileId,
            Title = dto.Title,
            Slug = dto.Slug,
            Summary = dto.Summary,
            CreatedAt = DateTime.UtcNow
        };

        // Use the same assignment helper as update so create/update behavior stays aligned.
        ApplyChanges(offering, dto);

        // Attach tags before the first save so the created response includes the final tag list.
        await UpdateOfferingTagsAsync(offering, dto.Tags);

        _context.Offerings.Add(offering);
        await _context.SaveChangesAsync();

        // Reload through the read path so the response shape matches every other owner detail response.
        return await GetMineByIdAsync(userId, offering.Id);
    }

    /// <inheritdoc/>
    public async Task<OfferingDto> UpdateAsync(int userId, int offeringId, UpsertOfferingDto dto)
    {
        // Resolve ownership first. The update query is scoped by ProfileId so users cannot edit each other's offerings.
        int profileId = await GetOwnedProfileIdAsync(userId);

        Offering offering = await _context
            .Offerings
            .Include(offering => offering.Tags)
            .FirstOrDefaultAsync(offering => offering.Id == offeringId && offering.ProfileId == profileId)
            ?? throw new KeyNotFoundException($"Offering with ID '{offeringId}' was not found.");

        // Exclude the current offering so an owner can save without changing the slug.
        await EnsureSlugIsAvailableAsync(profileId, dto.Slug, offeringId);

        // Keep all editable fields in one helper so create and update remain easy to compare.
        ApplyChanges(offering, dto);

        // Keep the tag collection in sync with the names submitted by the form.
        await UpdateOfferingTagsAsync(offering, dto.Tags);

        offering.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Return the projected DTO rather than the tracked entity.
        return await GetMineByIdAsync(userId, offering.Id);
    }

    /// <inheritdoc/>
    public async Task DeleteAsync(int userId, int offeringId)
    {
        // Deletes must be scoped just as tightly as updates. Resolve the owner profile first.
        int profileId = await GetOwnedProfileIdAsync(userId);

        Offering offering = await _context
            .Offerings
            .Include(offering => offering.Tags)
            .FirstOrDefaultAsync(offering => offering.Id == offeringId && offering.ProfileId == profileId)
            ?? throw new KeyNotFoundException($"Offering with ID '{offeringId}' was not found.");

        _context.Offerings.Remove(offering);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Applies owner-list offering filters before projection.
    /// </summary>
    /// <param name="query">The offering query already scoped to the authenticated owner.</param>
    /// <param name="filters">Filter values supplied from the request query string.</param>
    /// <returns>The filtered offering query.</returns>
    private static IQueryable<Offering> ApplyOfferingFilters(IQueryable<Offering> query, OfferingFilters filters)
    {
        // Status is about public visibility. Owners can still see drafts; this only narrows the list.
        query = filters.Status switch
        {
            OfferingStatus.Published => query.Where(offering => offering.IsPublished),
            OfferingStatus.Draft => query.Where(offering => !offering.IsPublished),
            _ => query
        };

        // Featured filtering is separate from status because an offering can be featured and still be a draft.
        query = filters.Featured switch
        {
            OfferingFeaturedFilter.Featured => query.Where(offering => offering.IsFeatured),
            OfferingFeaturedFilter.Regular => query.Where(offering => !offering.IsFeatured),
            _ => query
        };

        if (!string.IsNullOrWhiteSpace(filters.SearchTerm))
        {
            string searchTerm = filters.SearchTerm.Trim().ToLower();

            // Search the fields creators naturally remember: title, summary, body text, practical labels, and tags.
            query = query.Where(
                offering =>
                    offering.Title.ToLower().Contains(searchTerm)
                    || offering.Summary.ToLower().Contains(searchTerm)
                    || (offering.ContentText != null && offering.ContentText.ToLower().Contains(searchTerm))
                    || (offering.Price != null && offering.Price.ToLower().Contains(searchTerm))
                    || (offering.DeliveryMode != null && offering.DeliveryMode.ToLower().Contains(searchTerm))
                    || (offering.Duration != null && offering.Duration.ToLower().Contains(searchTerm))
                    || offering.Tags.Any(tag => tag.Name.ToLower().Contains(searchTerm))
            );
        }

        return query;
    }

    /// <summary>
    /// Applies the selected owner-list sort option to an offering query.
    /// </summary>
    /// <param name="query">The filtered offering query.</param>
    /// <param name="sortOption">The requested sort option.</param>
    /// <returns>The ordered offering query.</returns>
    private static IQueryable<Offering> ApplyOfferingSort(IQueryable<Offering> query, OfferingSortOption sortOption)
    {
        // Manual order mirrors the public profile order: featured offerings first, then the owner's SortOrder.
        if (sortOption == OfferingSortOption.Manual)
        {
            return query
                .OrderByDescending(offering => offering.IsFeatured)
                .ThenBy(offering => offering.SortOrder)
                .ThenBy(offering => offering.Title);
        }

        // Title sorting is useful when a creator is tidying a larger list of offerings.
        if (sortOption == OfferingSortOption.Title)
        {
            return query.OrderBy(offering => offering.Title);
        }

        // Date sorting uses UpdatedAt when available, then falls back to CreatedAt for untouched offerings.
        return sortOption == OfferingSortOption.Oldest
            ? query.OrderBy(offering => offering.UpdatedAt ?? offering.CreatedAt).ThenBy(offering => offering.Title)
            : query.OrderByDescending(offering => offering.UpdatedAt ?? offering.CreatedAt).ThenBy(offering => offering.Title);
    }

    /// <summary>
    /// Converts an offering query into the API response shape without loading full entity graphs.
    /// </summary>
    /// <param name="query">The offering query after ownership or public visibility filters have already been applied.</param>
    /// <returns>A projected query that returns offering DTOs.</returns>
    private static IQueryable<OfferingDto> OfferingDtos(IQueryable<Offering> query)
    {
        // Centralizing the projection keeps every endpoint consistent and prevents accidental over-fetching.
        return query.Select(offering => new OfferingDto
        {
            Id = offering.Id,
            ProfileId = offering.ProfileId,
            Title = offering.Title,
            Slug = offering.Slug,
            Summary = offering.Summary,

            // The frontend owns the rich editor and sends both HTML and plain text.
            ContentHtml = offering.ContentHtml,
            ContentText = offering.ContentText,

            Price = offering.Price,
            DeliveryMode = offering.DeliveryMode,
            Duration = offering.Duration,
            CallToAction = offering.CallToAction,
            CallToActionUrl = offering.CallToActionUrl,
            Tags = offering.Tags.Select(tag => tag.Name).ToList(),
            SortOrder = offering.SortOrder,
            IsFeatured = offering.IsFeatured,
            IsPublished = offering.IsPublished,
            SeoTitle = offering.SeoTitle,
            SeoDescription = offering.SeoDescription,
            CreatedAt = offering.CreatedAt,
            UpdatedAt = offering.UpdatedAt
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
        // Offerings are attached to a profile. Without a profile, there is nowhere to show them publicly.
        int? profileId = await _context
            .Profiles
            .AsNoTracking()
            .Where(profile => profile.UserId == userId)
            .Select(profile => (int?)profile.Id)
            .FirstOrDefaultAsync();

        // Keep this message helpful for the frontend instead of exposing database language.
        return profileId ?? throw new InvalidOperationException("Create your profile before adding offerings.");
    }

    /// <summary>
    /// Checks whether an offering slug can be used inside a specific profile.
    /// </summary>
    /// <param name="profileId">The profile that owns the offering.</param>
    /// <param name="slug">The URL-safe slug the user wants to save.</param>
    /// <param name="currentOfferingId">The existing offering id to ignore when updating an offering.</param>
    /// <exception cref="ConflictException">Thrown when another offering on the same profile already uses the slug.</exception>
    private async Task EnsureSlugIsAvailableAsync(int profileId, string slug, int? currentOfferingId = null)
    {
        // A slug only needs to be unique inside one profile, just like projects and blog posts.
        bool slugAlreadyInUse = await _context
            .Offerings
            .AnyAsync(
                offering =>
                    offering.ProfileId == profileId
                    && offering.Slug == slug
                    && (currentOfferingId == null || offering.Id != currentOfferingId.Value)
            );

        if (slugAlreadyInUse)
            throw new ConflictException("This offering link is already used on your profile.");
    }

    /// <summary>
    /// Updates the tag navigation collection on an offering from submitted tag names.
    /// </summary>
    /// <param name="offering">The tracked offering entity being created or updated.</param>
    /// <param name="newTagNames">The tag names submitted by the form.</param>
    private async Task UpdateOfferingTagsAsync(Offering offering, List<string> newTagNames)
    {
        // Distinct with a case-insensitive comparer prevents duplicates like "Tutoring" and "tutoring".
        var cleanedTagNames = newTagNames
            .Where(tagName => !string.IsNullOrWhiteSpace(tagName))
            .Select(tagName => tagName.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        // Remove tags that are no longer present in the submitted list.
        foreach (Tag tag in offering.Tags.ToList())
        {
            if (!cleanedTagNames.Any(tagName => tagName.Equals(tag.Name, StringComparison.OrdinalIgnoreCase)))
            {
                offering.Tags.Remove(tag);
            }
        }

        // Add missing tags through the tag service so existing tag rows are reused.
        foreach (string tagName in cleanedTagNames)
        {
            if (offering.Tags.Any(tag => tag.Name.Equals(tagName, StringComparison.OrdinalIgnoreCase)))
                continue;

            Tag tag = await _tagService.GetByNameAsync(tagName);
            offering.Tags.Add(tag);
        }
    }

    /// <summary>
    /// Copies editable fields from the incoming DTO onto an offering entity.
    /// </summary>
    /// <param name="offering">The offering entity being created or updated.</param>
    /// <param name="dto">The validated request data from the caller.</param>
    private static void ApplyChanges(Offering offering, UpsertOfferingDto dto)
    {
        // Keep all assignable fields here. It makes create and update easier to audit as the feature grows.
        offering.Title = dto.Title;
        offering.Slug = dto.Slug;
        offering.Summary = dto.Summary;

        // The frontend owns the rich editor and sends both HTML and plain text.
        offering.ContentHtml = dto.ContentHtml;
        offering.ContentText = dto.ContentText;

        // These fields help visitors understand what is offered before they contact the profile owner.
        offering.Price = dto.Price;
        offering.DeliveryMode = dto.DeliveryMode;
        offering.Duration = dto.Duration;
        offering.CallToAction = dto.CallToAction;
        offering.CallToActionUrl = dto.CallToActionUrl;

        // Tags are attached through ITagService so the same tag rows can be reused across content.

        // These fields control how the offering appears on the public profile.
        offering.SortOrder = dto.SortOrder;
        offering.IsFeatured = dto.IsFeatured;
        offering.IsPublished = dto.IsPublished;

        // SEO fields are optional overrides for public pages.
        offering.SeoTitle = dto.SeoTitle;
        offering.SeoDescription = dto.SeoDescription;
    }
}

