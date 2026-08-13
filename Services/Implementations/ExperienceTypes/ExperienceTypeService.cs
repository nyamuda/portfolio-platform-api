using Microsoft.EntityFrameworkCore;
using PortfolioPlatform.Api.Data;
using PortfolioPlatform.Api.Dtos.ExperienceTypes;
using PortfolioPlatform.Api.Enums.Experiences;
using PortfolioPlatform.Api.Exceptions;
using PortfolioPlatform.Api.Models;
using PortfolioPlatform.Api.Models.Content;
using PortfolioPlatform.Api.Services.Abstractions.ExperienceTypes;

namespace PortfolioPlatform.Api.Services.Implementations.ExperienceTypes;

/// <inheritdoc />
public class ExperienceTypeService(ApplicationDbContext context, ILogger<ExperienceTypeService> logger)
    : IExperienceTypeService
{
    private readonly ApplicationDbContext _context = context;
    private readonly ILogger<ExperienceTypeService> _logger = logger;

    /// <inheritdoc />
    public async Task<PageInfo<ExperienceTypeDto>> GetExperienceTypesAsync(
        ExperienceTypeQueryParams queryParams
    )
    {
        int page = Math.Max(queryParams.Page, 1);
        int pageSize = Math.Clamp(queryParams.PageSize, 1, 100);
        string? search = queryParams.Search?.Trim();

        IQueryable<ExperienceType> query = _context.ExperienceTypes.AsQueryable();

        // Type search checks both name and description so admins can find a type by meaning, not only exact label.
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(type =>
                type.Name.ToLower().Contains(search.ToLower())
                || (type.Description != null && type.Description.ToLower().Contains(search.ToLower()))
            );
        }

        query = queryParams.SortBy switch
        {
            ExperienceTypeSortOption.Name => query.OrderBy(type => type.Name),
            ExperienceTypeSortOption.New => query.OrderByDescending(type => type.CreatedAt),
            ExperienceTypeSortOption.Popularity => query.OrderByDescending(type => type.Experiences.Count),
            _ => query
                .OrderByDescending(type => type.IsFeatured)
                .ThenBy(type => type.SortOrder)
                .ThenBy(type => type.Name)
        };

        int totalItems = await query.CountAsync();

        List<ExperienceTypeDto> types = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .Select(type => new ExperienceTypeDto
            {
                Id = type.Id,
                Name = type.Name,
                Description = type.Description,
                Slug = type.Slug,
                ColorHex = type.ColorHex,
                IconName = type.IconName,
                IsFeatured = type.IsFeatured,
                SortOrder = type.SortOrder,
                CreatedAt = type.CreatedAt,
                UpdatedAt = type.UpdatedAt,
                TotalExperiences = type.Experiences.Count
            })
            .ToListAsync();

        return new PageInfo<ExperienceTypeDto>
        {
            Page = page,
            PageSize = pageSize,
            HasMore = totalItems > page * pageSize,
            TotalItems = totalItems,
            Items = types
        };
    }

    /// <inheritdoc />
    public async Task<ExperienceTypeDto> CreateAsync(UpsertExperienceTypeDto dto)
    {
        string cleanedName = CleanName(dto.Name);
        string slug = CreateSlug(string.IsNullOrWhiteSpace(dto.Slug) ? cleanedName : dto.Slug);

        // Type names and slugs are shared globally, so both need to stay unique.
        await EnsureTypeIdentityIsAvailableAsync(cleanedName, slug);

        ExperienceType type = new()
        {
            Name = cleanedName,
            Slug = slug,
            CreatedAt = DateTime.UtcNow
        };

        ApplyAdminChanges(type, dto, cleanedName, slug);

        _context.ExperienceTypes.Add(type);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created experience type: {ExperienceTypeName}", type.Name);

        return await GetByIdAsync(type.Id);
    }

    /// <inheritdoc />
    public async Task<ExperienceTypeDto> UpdateAsync(int experienceTypeId, UpsertExperienceTypeDto dto)
    {
        ExperienceType type = await _context
            .ExperienceTypes
            .FirstOrDefaultAsync(type => type.Id == experienceTypeId)
            ?? throw new KeyNotFoundException($"Experience type with ID '{experienceTypeId}' was not found.");

        string cleanedName = CleanName(dto.Name);
        string slug = CreateSlug(string.IsNullOrWhiteSpace(dto.Slug) ? cleanedName : dto.Slug);

        // Exclude the current type so admins can save unchanged names/slugs without false conflicts.
        await EnsureTypeIdentityIsAvailableAsync(cleanedName, slug, experienceTypeId);

        ApplyAdminChanges(type, dto, cleanedName, slug);
        type.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return await GetByIdAsync(type.Id);
    }

    /// <inheritdoc />
    public async Task DeleteAsync(int experienceTypeId)
    {
        ExperienceType type = await _context
            .ExperienceTypes
            .Include(type => type.Experiences)
            .FirstOrDefaultAsync(type => type.Id == experienceTypeId)
            ?? throw new KeyNotFoundException($"Experience type with ID '{experienceTypeId}' was not found.");

        // Do not silently detach timeline entries from their primary type. If a type is in use,
        // an admin should first move those experiences to another type and then delete it.
        if (type.Experiences.Count > 0)
            throw new ConflictException("This experience type is still used by timeline entries.");

        _context.ExperienceTypes.Remove(type);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Loads a single experience type by id and returns it in the normal frontend response shape.
    /// </summary>
    /// <param name="experienceTypeId">The experience type identifier.</param>
    /// <returns>The experience type response DTO.</returns>
    private async Task<ExperienceTypeDto> GetByIdAsync(int experienceTypeId)
    {
        return await _context
            .ExperienceTypes
            .AsNoTracking()
            .Where(type => type.Id == experienceTypeId)
            .Select(type => new ExperienceTypeDto
            {
                Id = type.Id,
                Name = type.Name,
                Description = type.Description,
                Slug = type.Slug,
                ColorHex = type.ColorHex,
                IconName = type.IconName,
                IsFeatured = type.IsFeatured,
                SortOrder = type.SortOrder,
                CreatedAt = type.CreatedAt,
                UpdatedAt = type.UpdatedAt,
                TotalExperiences = type.Experiences.Count
            })
            .FirstAsync();
    }

    /// <summary>
    /// Applies administrator-managed fields to an experience type entity.
    /// </summary>
    /// <param name="type">The experience type entity being created or updated.</param>
    /// <param name="dto">The request body supplied by the admin screen.</param>
    /// <param name="cleanedName">The normalised type name.</param>
    /// <param name="slug">The normalised type slug.</param>
    private static void ApplyAdminChanges(
        ExperienceType type,
        UpsertExperienceTypeDto dto,
        string cleanedName,
        string slug
    )
    {
        type.Name = cleanedName;
        type.Slug = slug;

        // Optional fields are trimmed so the database does not keep accidental whitespace.
        type.Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim();
        type.ColorHex = string.IsNullOrWhiteSpace(dto.ColorHex) ? null : dto.ColorHex.Trim();
        type.IconName = string.IsNullOrWhiteSpace(dto.IconName) ? null : dto.IconName.Trim();
        type.IsFeatured = dto.IsFeatured;
        type.SortOrder = dto.SortOrder;
    }

    /// <summary>
    /// Checks that an experience type name and slug are not already used by another type.
    /// </summary>
    /// <param name="name">The normalised type name.</param>
    /// <param name="slug">The normalised type slug.</param>
    /// <param name="currentTypeId">Existing type id to exclude during updates.</param>
    private async Task EnsureTypeIdentityIsAvailableAsync(
        string name,
        string slug,
        int? currentTypeId = null
    )
    {
        bool nameExists = await _context.ExperienceTypes.AnyAsync(type =>
            type.Name.ToLower() == name.ToLower()
            && (currentTypeId == null || type.Id != currentTypeId.Value)
        );

        if (nameExists)
            throw new ConflictException("An experience type with this name already exists.");

        bool slugExists = await _context.ExperienceTypes.AnyAsync(type =>
            type.Slug != null
            && type.Slug.ToLower() == slug.ToLower()
            && (currentTypeId == null || type.Id != currentTypeId.Value)
        );

        if (slugExists)
            throw new ConflictException("An experience type with this slug already exists.");
    }

    /// <summary>
    /// Cleans a type name before it is compared, saved, or returned.
    /// </summary>
    /// <param name="name">Raw type text supplied by a caller.</param>
    /// <returns>A trimmed type name with repeated whitespace collapsed.</returns>
    private static string CleanName(string name)
    {
        return string.Join(' ', name.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }

    /// <summary>
    /// Creates a URL-friendly slug from a type name or admin-supplied slug.
    /// </summary>
    /// <param name="value">The raw value to convert into a slug.</param>
    /// <returns>A lowercase, hyphen-separated slug.</returns>
    private static string CreateSlug(string value)
    {
        string cleaned = string.Join(' ', value.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries));
        string slug = new(cleaned
            .ToLowerInvariant()
            .Select(character => char.IsLetterOrDigit(character) ? character : '-')
            .ToArray());

        return string.Join('-', slug.Split('-', StringSplitOptions.RemoveEmptyEntries));
    }
}
