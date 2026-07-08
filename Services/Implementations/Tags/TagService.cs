using Microsoft.EntityFrameworkCore;
using PortfolioPlatform.Api.Data;
using PortfolioPlatform.Api.Dtos.Tags;
using PortfolioPlatform.Api.Enums.Tags;
using PortfolioPlatform.Api.Exceptions;
using PortfolioPlatform.Api.Models;
using PortfolioPlatform.Api.Models.Content;
using PortfolioPlatform.Api.Services.Abstractions.Tags;

namespace PortfolioPlatform.Api.Services.Implementations.Tags;

/// <inheritdoc />
public class TagService(ApplicationDbContext context, ILogger<TagService> logger) : ITagService
{
    private readonly ApplicationDbContext _context = context;
    private readonly ILogger<TagService> _logger = logger;

    /// <inheritdoc />
    public async Task<Tag> GetByNameAsync(string name)
    {
        // Normalise the incoming value once so every lookup and insert follows the same rule.
        string cleanedName = CleanTagName(name);

        Tag? tag = await _context
            .Tags
            .FirstOrDefaultAsync(existingTag => existingTag.Name.ToLower().Equals(cleanedName.ToLower()));

        if (tag is not null)
        {
            return tag;
        }

        // If the tag does not exist yet, create the smallest valid record immediately.
        // Admin screens can add descriptions, colours, icons, and featured status later.
        tag = new Tag
        {
            Name = cleanedName,
            Slug = CreateSlug(cleanedName)
        };

        _context.Tags.Add(tag);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created portfolio tag: {TagName}", tag.Name);

        return tag;
    }

    /// <inheritdoc />
    public async Task EnsureTagsExistAsync(IEnumerable<string> tagNames)
    {
        // Empty or repeated tag values should not create unnecessary database work.
        List<string> cleanedNames = tagNames
            .Select(CleanTagName)
            .Where(tagName => !string.IsNullOrWhiteSpace(tagName))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        foreach (string tagName in cleanedNames)
        {
            await GetByNameAsync(tagName);
        }
    }

    /// <inheritdoc />
    public async Task<PageInfo<TagDto>> GetTagsAsync(TagQueryParams queryParams)
    {
        int page = Math.Max(queryParams.Page, 1);
        int pageSize = Math.Clamp(queryParams.PageSize, 1, 100);
        string? search = queryParams.Search?.Trim();

        IQueryable<Tag> query = _context.Tags.AsQueryable();

        // Keep search simple and predictable: users type a word, and the API returns tags
        // whose names contain that word.
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(tag => tag.Name.ToLower().Contains(search.ToLower()));
        }

        query = queryParams.SortBy switch
        {
            TagSortOption.Name => query.OrderBy(tag => tag.Name),
            TagSortOption.New => query.OrderByDescending(tag => tag.CreatedAt),
            _ => query.OrderByDescending(tag => tag.Projects.Count + tag.BlogPosts.Count)
        };

        int totalItems = await query.CountAsync();

        List<TagDto> tags = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .Select(tag => new TagDto
            {
                Id = tag.Id,
                Name = tag.Name,
                Description = tag.Description,
                Slug = tag.Slug,
                ColorHex = tag.ColorHex,
                IconName = tag.IconName,
                IsFeatured = tag.IsFeatured,
                CreatedAt = tag.CreatedAt,
                UpdatedAt = tag.UpdatedAt,
                TotalProjects = tag.Projects.Count,
                TotalBlogPosts = tag.BlogPosts.Count,
                TotalUses = tag.Projects.Count + tag.BlogPosts.Count,
            })
            .ToListAsync();

        return new PageInfo<TagDto>
        {
            Page = page,
            PageSize = pageSize,
            HasMore = totalItems > page * pageSize,
            TotalItems = totalItems,
            Items = tags
        };
    }

    /// <inheritdoc />
    public async Task<TagDto> CreateAsync(UpsertTagDto dto)
    {
        string cleanedName = CleanTagName(dto.Name);
        string slug = CreateSlug(string.IsNullOrWhiteSpace(dto.Slug) ? cleanedName : dto.Slug);

        // Tag names and slugs are shared across the whole platform, so both must be unique.
        await EnsureTagIdentityIsAvailableAsync(cleanedName, slug);

        Tag tag = new()
        {
            Name = cleanedName,
            Slug = slug,
            CreatedAt = DateTime.UtcNow
        };

        // Keep optional visual/display fields in one helper so create and update stay aligned.
        ApplyAdminChanges(tag, dto, cleanedName, slug);

        _context.Tags.Add(tag);
        await _context.SaveChangesAsync();

        return await GetByIdAsync(tag.Id);
    }

    /// <inheritdoc />
    public async Task<TagDto> UpdateAsync(int tagId, UpsertTagDto dto)
    {
        Tag tag = await _context
            .Tags
            .FirstOrDefaultAsync(tag => tag.Id == tagId)
            ?? throw new KeyNotFoundException($"Tag with ID '{tagId}' was not found.");

        string cleanedName = CleanTagName(dto.Name);
        string slug = CreateSlug(string.IsNullOrWhiteSpace(dto.Slug) ? cleanedName : dto.Slug);

        // Exclude the current tag so admins can save unchanged names/slugs without false conflicts.
        await EnsureTagIdentityIsAvailableAsync(cleanedName, slug, tagId);

        ApplyAdminChanges(tag, dto, cleanedName, slug);
        tag.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return await GetByIdAsync(tag.Id);
    }

    /// <inheritdoc />
    public async Task DeleteAsync(int tagId)
    {
        Tag tag = await _context
            .Tags
            .Include(tag => tag.Projects)
            .Include(tag => tag.BlogPosts)
            .FirstOrDefaultAsync(tag => tag.Id == tagId)
            ?? throw new KeyNotFoundException($"Tag with ID '{tagId}' was not found.");

        // Clear the join collections deliberately. EF would remove join rows for us when the tag
        // is deleted, but being explicit makes the ownership rule obvious to future maintainers:
        // content survives, only the shared label is removed.
        tag.Projects.Clear();
        tag.BlogPosts.Clear();

        _context.Tags.Remove(tag);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Loads a single tag by id and returns it in the normal frontend response shape.
    /// </summary>
    /// <param name="tagId">The tag identifier.</param>
    /// <returns>The tag response DTO.</returns>
    private async Task<TagDto> GetByIdAsync(int tagId)
    {
        return await _context
            .Tags
            .AsNoTracking()
            .Where(tag => tag.Id == tagId)
            .Select(tag => new TagDto
            {
                Id = tag.Id,
                Name = tag.Name,
                Description = tag.Description,
                Slug = tag.Slug,
                ColorHex = tag.ColorHex,
                IconName = tag.IconName,
                IsFeatured = tag.IsFeatured,
                CreatedAt = tag.CreatedAt,
                UpdatedAt = tag.UpdatedAt,
                TotalProjects = tag.Projects.Count,
                TotalBlogPosts = tag.BlogPosts.Count,
                TotalUses = tag.Projects.Count + tag.BlogPosts.Count,
            })
            .FirstAsync();
    }

    /// <summary>
    /// Applies administrator-managed fields to a tag entity.
    /// </summary>
    /// <param name="tag">The tag entity being created or updated.</param>
    /// <param name="dto">The request body supplied by the admin screen.</param>
    /// <param name="cleanedName">The normalised tag name.</param>
    /// <param name="slug">The normalised tag slug.</param>
    private static void ApplyAdminChanges(Tag tag, UpsertTagDto dto, string cleanedName, string slug)
    {
        // Required identity fields are cleaned before this helper is called.
        tag.Name = cleanedName;
        tag.Slug = slug;

        // Optional fields are trimmed so the database does not fill up with accidental spaces.
        tag.Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim();
        tag.ColorHex = string.IsNullOrWhiteSpace(dto.ColorHex) ? null : dto.ColorHex.Trim();
        tag.IconName = string.IsNullOrWhiteSpace(dto.IconName) ? null : dto.IconName.Trim();
        tag.IsFeatured = dto.IsFeatured;
    }

    /// <summary>
    /// Checks that a tag name and slug are not already used by another tag.
    /// </summary>
    /// <param name="name">The normalised tag name.</param>
    /// <param name="slug">The normalised tag slug.</param>
    /// <param name="currentTagId">Existing tag id to exclude during updates.</param>
    private async Task EnsureTagIdentityIsAvailableAsync(string name, string slug, int? currentTagId = null)
    {
        bool nameExists = await _context.Tags.AnyAsync(tag =>
            tag.Name.ToLower() == name.ToLower()
            && (currentTagId == null || tag.Id != currentTagId.Value)
        );

        if (nameExists)
            throw new ConflictException("A tag with this name already exists.");

        bool slugExists = await _context.Tags.AnyAsync(tag =>
            tag.Slug != null
            && tag.Slug.ToLower() == slug.ToLower()
            && (currentTagId == null || tag.Id != currentTagId.Value)
        );

        if (slugExists)
            throw new ConflictException("A tag with this slug already exists.");
    }

    /// <summary>
    /// Converts a tag entity into the frontend response shape.
    /// </summary>
    /// <param name="tag">The tag entity from the query projection.</param>
    /// <returns>A tag DTO with metadata and usage counts.</returns>
    private static TagDto ToTagDto(Tag tag) =>
        new()
        {
            Id = tag.Id,
            Name = tag.Name,
            Description = tag.Description,
            Slug = tag.Slug,
            ColorHex = tag.ColorHex,
            IconName = tag.IconName,
            IsFeatured = tag.IsFeatured,
            CreatedAt = tag.CreatedAt,
            UpdatedAt = tag.UpdatedAt,
            TotalProjects = tag.Projects.Count,
            TotalBlogPosts = tag.BlogPosts.Count,
            TotalUses = tag.Projects.Count + tag.BlogPosts.Count
        };

    /// <summary>
    /// Cleans a tag name before it is compared, saved, or returned.
    /// </summary>
    /// <param name="name">Raw tag text supplied by a caller.</param>
    /// <returns>A trimmed tag name with repeated whitespace collapsed.</returns>
    private static string CleanTagName(string name)
    {
        // Split and join handles accidental double spaces while preserving readable tag names.
        return string.Join(' ', name.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }

    /// <summary>
    /// Creates a URL-friendly slug from a tag name or admin-supplied slug.
    /// </summary>
    /// <param name="value">The raw value to convert into a slug.</param>
    /// <returns>A lowercase, hyphen-separated slug.</returns>
    private static string CreateSlug(string value)
    {
        // Keep the slug generator small and predictable. It is enough for tag names, and it avoids
        // leaking spaces or punctuation into future public tag URLs.
        string cleaned = string.Join(' ', value.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries));
        string slug = new(cleaned
            .ToLowerInvariant()
            .Select(character => char.IsLetterOrDigit(character) ? character : '-')
            .ToArray());

        return string.Join('-', slug.Split('-', StringSplitOptions.RemoveEmptyEntries));
    }
}

