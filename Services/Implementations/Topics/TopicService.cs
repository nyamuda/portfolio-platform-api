using Microsoft.EntityFrameworkCore;
using PortfolioPlatform.Api.Data;
using PortfolioPlatform.Api.Dtos.Topics;
using PortfolioPlatform.Api.Enums.Topics;
using PortfolioPlatform.Api.Exceptions;
using PortfolioPlatform.Api.Models;
using PortfolioPlatform.Api.Models.Content;
using PortfolioPlatform.Api.Services.Abstractions.Topics;

namespace PortfolioPlatform.Api.Services.Implementations.Topics;

/// <inheritdoc />
public class TopicService(ApplicationDbContext context, ILogger<TopicService> logger) : ITopicService
{
    private readonly ApplicationDbContext _context = context;
    private readonly ILogger<TopicService> _logger = logger;

    /// <inheritdoc />
    public async Task<PageInfo<TopicDto>> GetTopicsAsync(TopicQueryParams queryParams)
    {
        int page = Math.Max(queryParams.Page, 1);
        int pageSize = Math.Clamp(queryParams.PageSize, 1, 100);
        string? search = queryParams.Search?.Trim();

        IQueryable<Topic> query = _context.Topics.AsQueryable();

        // Topic search checks both the name and description so admins can find topics by meaning,
        // not only by the exact label that appears in the UI.
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(topic =>
                topic.Name.ToLower().Contains(search.ToLower())
                || (topic.Description != null && topic.Description.ToLower().Contains(search.ToLower()))
            );
        }

        query = queryParams.SortBy switch
        {
            TopicSortOption.Name => query.OrderBy(topic => topic.Name),
            TopicSortOption.New => query.OrderByDescending(topic => topic.CreatedAt),
            _ => query.OrderByDescending(topic => topic.BlogPosts.Count)
        };

        int totalItems = await query.CountAsync();

        List<TopicDto> topics = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .Select(topic => new TopicDto
            {
                Id = topic.Id,
                Name = topic.Name,
                Description = topic.Description,
                Slug = topic.Slug,
                ColorHex = topic.ColorHex,
                IconName = topic.IconName,
                IsFeatured = topic.IsFeatured,
                CreatedAt = topic.CreatedAt,
                UpdatedAt = topic.UpdatedAt,
                TotalBlogPosts = topic.BlogPosts.Count
            })
            .ToListAsync();

        return new PageInfo<TopicDto>
        {
            Page = page,
            PageSize = pageSize,
            HasMore = totalItems > page * pageSize,
            TotalItems = totalItems,
            Items = topics
        };
    }

    /// <inheritdoc />
    public async Task<TopicDto> CreateAsync(UpsertTopicDto dto)
    {
        string cleanedName = CleanTopicName(dto.Name);
        string slug = CreateSlug(string.IsNullOrWhiteSpace(dto.Slug) ? cleanedName : dto.Slug);

        // Topic names and slugs are shared globally, so both need to stay unique.
        await EnsureTopicIdentityIsAvailableAsync(cleanedName, slug);

        Topic topic = new()
        {
            Name = cleanedName,
            Slug = slug,
            CreatedAt = DateTime.UtcNow
        };

        ApplyAdminChanges(topic, dto, cleanedName, slug);

        _context.Topics.Add(topic);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created blog topic: {TopicName}", topic.Name);

        return await GetByIdAsync(topic.Id);
    }

    /// <inheritdoc />
    public async Task<TopicDto> UpdateAsync(int topicId, UpsertTopicDto dto)
    {
        Topic topic = await _context
            .Topics
            .FirstOrDefaultAsync(topic => topic.Id == topicId)
            ?? throw new KeyNotFoundException($"Topic with ID '{topicId}' was not found.");

        string cleanedName = CleanTopicName(dto.Name);
        string slug = CreateSlug(string.IsNullOrWhiteSpace(dto.Slug) ? cleanedName : dto.Slug);

        // Exclude the current topic so admins can save unchanged names/slugs without false conflicts.
        await EnsureTopicIdentityIsAvailableAsync(cleanedName, slug, topicId);

        ApplyAdminChanges(topic, dto, cleanedName, slug);
        topic.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return await GetByIdAsync(topic.Id);
    }

    /// <inheritdoc />
    public async Task DeleteAsync(int topicId)
    {
        Topic topic = await _context
            .Topics
            .Include(topic => topic.BlogPosts)
            .FirstOrDefaultAsync(topic => topic.Id == topicId)
            ?? throw new KeyNotFoundException($"Topic with ID '{topicId}' was not found.");

        // Blog posts should survive when an admin removes a topic. We clear the topic link first
        // so older posts simply become uncategorised instead of being deleted.
        foreach (BlogPost post in topic.BlogPosts)
        {
            post.TopicId = null;
            post.Topic = null;
        }

        _context.Topics.Remove(topic);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Loads a single topic by id and returns it in the normal frontend response shape.
    /// </summary>
    /// <param name="topicId">The topic identifier.</param>
    /// <returns>The topic response DTO.</returns>
    private async Task<TopicDto> GetByIdAsync(int topicId)
    {
        return await _context
            .Topics
            .AsNoTracking()
            .Where(topic => topic.Id == topicId)
            .Select(topic => new TopicDto
            {
                Id = topic.Id,
                Name = topic.Name,
                Description = topic.Description,
                Slug = topic.Slug,
                ColorHex = topic.ColorHex,
                IconName = topic.IconName,
                IsFeatured = topic.IsFeatured,
                CreatedAt = topic.CreatedAt,
                UpdatedAt = topic.UpdatedAt,
                TotalBlogPosts = topic.BlogPosts.Count
            })
            .FirstAsync();
    }

    /// <summary>
    /// Applies administrator-managed fields to a topic entity.
    /// </summary>
    /// <param name="topic">The topic entity being created or updated.</param>
    /// <param name="dto">The request body supplied by the admin screen.</param>
    /// <param name="cleanedName">The normalised topic name.</param>
    /// <param name="slug">The normalised topic slug.</param>
    private static void ApplyAdminChanges(Topic topic, UpsertTopicDto dto, string cleanedName, string slug)
    {
        topic.Name = cleanedName;
        topic.Slug = slug;

        // Optional fields are trimmed so the database does not keep accidental whitespace.
        topic.Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim();
        topic.ColorHex = string.IsNullOrWhiteSpace(dto.ColorHex) ? null : dto.ColorHex.Trim();
        topic.IconName = string.IsNullOrWhiteSpace(dto.IconName) ? null : dto.IconName.Trim();
        topic.IsFeatured = dto.IsFeatured;
    }

    /// <summary>
    /// Checks that a topic name and slug are not already used by another topic.
    /// </summary>
    /// <param name="name">The normalised topic name.</param>
    /// <param name="slug">The normalised topic slug.</param>
    /// <param name="currentTopicId">Existing topic id to exclude during updates.</param>
    private async Task EnsureTopicIdentityIsAvailableAsync(string name, string slug, int? currentTopicId = null)
    {
        bool nameExists = await _context.Topics.AnyAsync(topic =>
            topic.Name.ToLower() == name.ToLower()
            && (currentTopicId == null || topic.Id != currentTopicId.Value)
        );

        if (nameExists)
            throw new ConflictException("A topic with this name already exists.");

        bool slugExists = await _context.Topics.AnyAsync(topic =>
            topic.Slug != null
            && topic.Slug.ToLower() == slug.ToLower()
            && (currentTopicId == null || topic.Id != currentTopicId.Value)
        );

        if (slugExists)
            throw new ConflictException("A topic with this slug already exists.");
    }

    /// <summary>
    /// Cleans a topic name before it is compared, saved, or returned.
    /// </summary>
    /// <param name="name">Raw topic text supplied by a caller.</param>
    /// <returns>A trimmed topic name with repeated whitespace collapsed.</returns>
    private static string CleanTopicName(string name)
    {
        return string.Join(' ', name.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }

    /// <summary>
    /// Creates a URL-friendly slug from a topic name or admin-supplied slug.
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
