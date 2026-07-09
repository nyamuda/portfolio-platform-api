using Microsoft.EntityFrameworkCore;
using PortfolioPlatform.Api.Data;
using PortfolioPlatform.Api.Dtos.BlogPosts;
using PortfolioPlatform.Api.Enums.BlogPosts;
using PortfolioPlatform.Api.Exceptions;
using PortfolioPlatform.Api.Models;
using PortfolioPlatform.Api.Models.Content;
using PortfolioPlatform.Api.Services.Abstractions.BlogPosts;
using PortfolioPlatform.Api.Services.Abstractions.Tags;

namespace PortfolioPlatform.Api.Services.Implementations.BlogPosts;

/// <summary>
/// Handles blog post management for profile owners and public visitors.
/// </summary>
public class BlogPostService(ApplicationDbContext context, ITagService tagService) : IBlogPostService
{
    private readonly ApplicationDbContext _context = context;
    private readonly ITagService _tagService = tagService;
    /// <inheritdoc/>
    public async Task<PageInfo<BlogPostDto>> GetMineAsync(int userId, BlogPostFilters filters)
    {
        // Blog posts belong to profiles, not directly to users. This lookup also proves ownership.
        int profileId = await GetOwnedProfileIdAsync(userId);

        // Keep pagination defensive. The frontend owns the controls, but the API should still protect itself.
        int page = Math.Max(filters.Page, 1);
        int pageSize = Math.Clamp(filters.PageSize, 1, 50);

        // Owner reads include drafts because the editor and dashboard need to show unfinished posts.
        // Public endpoints apply stricter filters separately, so drafts do not leak to visitors.
        IQueryable<BlogPost> query = _context
            .BlogPosts
            .AsNoTracking()
            .Where(post => post.ProfileId == profileId);

        // Filters are applied before the count so the paginator describes the visible result set.
        query = ApplyBlogPostFilters(query, filters);
        int totalItems = await query.CountAsync();

        // Sorting is applied after filtering, then the requested page is projected into DTOs.
        query = ApplyBlogPostSort(query, filters.SortBy);
        List<BlogPostDto> posts = await BlogPostDtos(query)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PageInfo<BlogPostDto>
        {
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            HasMore = totalItems > page * pageSize,
            Items = posts
        };
    }
    /// <inheritdoc/>
    public async Task<BlogPostDto> GetMineByIdAsync(int userId, int postId)
    {
        // Resolve the profile first so the post lookup cannot accidentally return another user's post.
        int profileId = await GetOwnedProfileIdAsync(userId);

        // Keep this query projected as well. Even detail views do not need the whole Profile/User graph.
        return await BlogPostDtos(
                _context
                    .BlogPosts
                    .AsNoTracking()
                    .Where(post => post.Id == postId && post.ProfileId == profileId)
            )
            .FirstOrDefaultAsync()
            ?? throw new KeyNotFoundException($"Blog post with ID '{postId}' was not found.");
    }

    /// <inheritdoc/>
    public async Task<List<BlogPostDto>> GetPublicByProfileSlugAsync(string profileSlug)
    {
        // Public reads are intentionally stricter than owner reads:
        // the profile must be published and the individual post must also be published.
        return await BlogPostDtos(
                _context
                    .BlogPosts
                    .AsNoTracking()
                    .Where(
                        post =>
                            post.IsPublished
                            && post.Profile.IsPublished
                            && post.Profile.Slug == profileSlug
                    )
            )
            // Public ordering mirrors the owner ordering so the public profile feels predictable.
            .OrderByDescending(post => post.IsFeatured)
            .ThenBy(post => post.SortOrder)
            .ThenByDescending(post => post.CreatedAt)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<BlogPostDto> GetPublicBySlugAsync(string profileSlug, string postSlug)
    {
        // The profile slug is part of the lookup because post slugs are only unique within one profile.
        // That allows two different users to publish posts with the same friendly URL slug.
        return await BlogPostDtos(
                _context
                    .BlogPosts
                    .AsNoTracking()
                    .Where(
                        post =>
                            post.Slug == postSlug
                            && post.IsPublished
                            && post.Profile.IsPublished
                            && post.Profile.Slug == profileSlug
                    )
            )
            .FirstOrDefaultAsync()
            ?? throw new KeyNotFoundException($"Blog post with slug '{postSlug}' was not found.");
    }

    /// <inheritdoc/>
    public async Task<BlogPostDto> CreateAsync(int userId, UpsertBlogPostDto dto)
    {
        // A post needs a profile because public routes are profile-based.
        int profileId = await GetOwnedProfileIdAsync(userId);

        // Slugs become public URLs, so we stop duplicates before inserting the record.
        await EnsureSlugIsAvailableAsync(profileId, dto.Slug);

        // Topic selection is optional, but when a topic id is supplied it should point to a real topic.
        await EnsureTopicExistsAsync(dto.TopicId);

        // Set the required identity fields first. ApplyChanges fills the editable content fields below.
        BlogPost post = new()
        {
            ProfileId = profileId,
            Title = dto.Title,
            Slug = dto.Slug,
            Excerpt = dto.Excerpt,
            CreatedAt = DateTime.UtcNow
        };

        // Reuse the same field assignment path as updates so create/update behavior stays consistent.
        ApplyChanges(post, dto);

        // Resolve the submitted tag names into shared Tag rows before the first save.
        // This keeps the public model relational while allowing forms to submit simple strings.
        await UpdateBlogPostTagsAsync(post, dto.Tags);

        _context.BlogPosts.Add(post);
        await _context.SaveChangesAsync();

        // Reload through the read path so the response shape matches every other owner detail response.
        return await GetMineByIdAsync(userId, post.Id);
    }

    /// <inheritdoc/>
    public async Task<BlogPostDto> UpdateAsync(int userId, int postId, UpsertBlogPostDto dto)
    {
        // Resolve ownership first. The update query is scoped by ProfileId so users cannot edit each other's posts.
        int profileId = await GetOwnedProfileIdAsync(userId);

        BlogPost post = await _context
            .BlogPosts
            .Include(post => post.Tags)
            .FirstOrDefaultAsync(post => post.Id == postId && post.ProfileId == profileId)
            ?? throw new KeyNotFoundException($"Blog post with ID '{postId}' was not found.");

        // The current post is excluded from this check so a user can save without changing the slug.
        await EnsureSlugIsAvailableAsync(profileId, dto.Slug, postId);

        // Topic selection is optional, but stale topic ids should be rejected before saving.
        await EnsureTopicExistsAsync(dto.TopicId);

        // Keep all editable field assignments together so we do not forget fields between create and update.
        ApplyChanges(post, dto);

        // Keep the tag collection in sync with the names submitted by the editor.
        await UpdateBlogPostTagsAsync(post, dto.Tags);
        post.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Return the projected DTO rather than the tracked entity.
        return await GetMineByIdAsync(userId, post.Id);
    }

    /// <inheritdoc/>
    public async Task DeleteAsync(int userId, int postId)
    {
        // Resolve ownership before deleting. Deletes must be scoped just as tightly as updates.
        int profileId = await GetOwnedProfileIdAsync(userId);

        BlogPost post = await _context
            .BlogPosts
            .Include(post => post.Tags)
            .FirstOrDefaultAsync(post => post.Id == postId && post.ProfileId == profileId)
            ?? throw new KeyNotFoundException($"Blog post with ID '{postId}' was not found.");

        _context.BlogPosts.Remove(post);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Converts a blog post query into the API response shape without loading full entity graphs.
    /// </summary>
    /// <param name="query">The blog post query after ownership or public visibility filters have already been applied.</param>
    /// <returns>A projected query that returns blog post DTOs.</returns>
    /// <summary>
    /// Applies owner-list filters to a blog post query before pagination.
    /// </summary>
    /// <param name="query">The blog post query scoped to the authenticated user's profile.</param>
    /// <param name="filters">The filters requested by the owner-facing blog post list.</param>
    /// <returns>The filtered blog post query.</returns>
    private static IQueryable<BlogPost> ApplyBlogPostFilters(IQueryable<BlogPost> query, BlogPostFilters filters)
    {
        // Status is an owner-only filter. Drafts should stay available here, but the owner can hide them.
        query = filters.Status switch
        {
            BlogPostStatus.Published => query.Where(post => post.IsPublished),
            BlogPostStatus.Draft => query.Where(post => !post.IsPublished),
            _ => query
        };

        // Featured filtering helps owners quickly check which posts are being highlighted publicly.
        query = filters.Featured switch
        {
            BlogPostFeaturedFilter.Featured => query.Where(post => post.IsFeatured),
            BlogPostFeaturedFilter.Regular => query.Where(post => !post.IsFeatured),
            _ => query
        };

        if (filters.TopicId.HasValue)
        {
            // Topic filters are useful once a writer has a larger archive and wants to focus on one writing area.
            query = query.Where(post => post.TopicId == filters.TopicId.Value);
        }

        if (!string.IsNullOrWhiteSpace(filters.SearchTerm))
        {
            string searchTerm = filters.SearchTerm.Trim().ToLower();

            // Keep the search broad enough for management screens without pulling the post body into memory.
            query = query.Where(post =>
                post.Title.ToLower().Contains(searchTerm)
                || post.Excerpt.ToLower().Contains(searchTerm)
                || (post.Topic != null && post.Topic.Name.ToLower().Contains(searchTerm))
                || (post.ContentText != null && post.ContentText.ToLower().Contains(searchTerm))
                || post.Tags.Any(tag => tag.Name.ToLower().Contains(searchTerm))
            );
        }

        return query;
    }

    /// <summary>
    /// Applies the selected owner-list sort order to a blog post query.
    /// </summary>
    /// <param name="query">The filtered blog post query.</param>
    /// <param name="sortOption">The sort option selected by the owner.</param>
    /// <returns>The ordered blog post query.</returns>
    private static IQueryable<BlogPost> ApplyBlogPostSort(IQueryable<BlogPost> query, BlogPostSortOption sortOption)
    {
        return sortOption switch
        {
            // Manual keeps featured posts first, then respects the owner's SortOrder value.
            BlogPostSortOption.Manual => query
                .OrderByDescending(post => post.IsFeatured)
                .ThenBy(post => post.SortOrder)
                .ThenBy(post => post.Title),

            // Title sorting is useful when the owner is quickly scanning a larger archive.
            BlogPostSortOption.Title => query.OrderBy(post => post.Title),

            // Oldest is helpful when cleaning up early drafts or older writing.
            BlogPostSortOption.Oldest => query
                .OrderBy(post => post.UpdatedAt ?? post.CreatedAt)
                .ThenBy(post => post.Title),

            // Recent is the default because new or recently edited posts are usually what the owner needs first.
            _ => query
                .OrderByDescending(post => post.UpdatedAt ?? post.CreatedAt)
                .ThenBy(post => post.Title)
        };
    }
    private static IQueryable<BlogPostDto> BlogPostDtos(IQueryable<BlogPost> query)
    {
        // This method centralizes the projection so all blog endpoints return the same shape.
        // It also prevents accidental over-fetching as the model grows over time.
        return query.Select(post => new BlogPostDto
        {
            Id = post.Id,
            ProfileId = post.ProfileId,
            Title = post.Title,
            Slug = post.Slug,
            Excerpt = post.Excerpt,
            ContentHtml = post.ContentHtml,
            ContentText = post.ContentText,
            TopicId = post.TopicId,
            TopicName = post.Topic != null ? post.Topic.Name : null,
            TopicSlug = post.Topic != null ? post.Topic.Slug : null,
            Tags = post.Tags.Select(tag => tag.Name).ToList(),
            CoverImageUrl = post.CoverImageUrl,
            SortOrder = post.SortOrder,
            IsFeatured = post.IsFeatured,
            IsPublished = post.IsPublished,
            SeoTitle = post.SeoTitle,
            SeoDescription = post.SeoDescription,
            CreatedAt = post.CreatedAt,
            UpdatedAt = post.UpdatedAt
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
        // Posts are attached to a profile. If a user has not created a profile yet,
        // there is nowhere sensible to display a post publicly.
        int? profileId = await _context
            .Profiles
            .AsNoTracking()
            .Where(profile => profile.UserId == userId)
            .Select(profile => (int?)profile.Id)
            .FirstOrDefaultAsync();

        // The exception message is written for the API caller/frontend, not for database debugging.
        return profileId ?? throw new InvalidOperationException("Create your profile before adding blog posts.");
    }

    /// <summary>
    /// Checks whether a blog post slug can be used inside a specific profile.
    /// </summary>
    /// <param name="profileId">The profile that owns the blog post.</param>
    /// <param name="slug">The URL-safe slug the user wants to save.</param>
    /// <param name="currentPostId">The existing post id to ignore when updating a post.</param>
    /// <exception cref="ConflictException">Thrown when another post on the same profile already uses the slug.</exception>
    private async Task EnsureSlugIsAvailableAsync(int profileId, string slug, int? currentPostId = null)
    {
        // A slug only needs to be unique inside one profile. This keeps URLs friendly without forcing
        // every creator on the platform to avoid titles that another creator has already used.
        bool slugAlreadyInUse = await _context
            .BlogPosts
            .AnyAsync(
                post =>
                    post.ProfileId == profileId
                    && post.Slug == slug
                    && (currentPostId == null || post.Id != currentPostId.Value)
            );

        if (slugAlreadyInUse)
            throw new ConflictException("This blog post link is already used on your profile.");
    }

    /// <summary>
    /// Checks that an optional topic id points to an existing topic before a blog post is saved.
    /// </summary>
    /// <param name="topicId">The optional managed topic selected by the writer.</param>
    private async Task EnsureTopicExistsAsync(int? topicId)
    {
        // No topic is perfectly valid. A writer can publish a post without choosing a topic yet.
        if (!topicId.HasValue)
            return;

        bool topicExists = await _context.Topics.AnyAsync(topic => topic.Id == topicId.Value);

        if (!topicExists)
            throw new KeyNotFoundException($"Topic with ID '{topicId.Value}' was not found.");
    }

    /// <summary>
    /// Synchronises a blog post's tag collection with the tag names submitted by the editor.
    /// </summary>
    /// <param name="post">The tracked blog post whose tag collection should be updated.</param>
    /// <param name="newTagNames">The tag names submitted by the form.</param>
    private async Task UpdateBlogPostTagsAsync(BlogPost post, List<string> newTagNames)
    {
        // Remove blank entries and collapse duplicates such as "Vue" and "vue" before touching the database.
        List<string> cleanedTagNames = newTagNames
            .Where(tagName => !string.IsNullOrWhiteSpace(tagName))
            .Select(tagName => tagName.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        // Remove tag links that are no longer present in the submitted list.
        // The Tag rows themselves stay in the database because other projects or posts may still use them.
        foreach (Tag tag in post.Tags.ToList())
        {
            if (!cleanedTagNames.Any(tagName => tagName.Equals(tag.Name, StringComparison.OrdinalIgnoreCase)))
            {
                post.Tags.Remove(tag);
            }
        }

        // Add any new tag names by reusing existing Tag rows when possible.
        foreach (string tagName in cleanedTagNames)
        {
            if (post.Tags.Any(tag => tag.Name.Equals(tagName, StringComparison.OrdinalIgnoreCase)))
                continue;

            Tag tag = await _tagService.GetByNameAsync(tagName); // creates the tag if this is the first use
            post.Tags.Add(tag);
        }
    }

    /// <summary>
    /// Copies editable fields from the incoming DTO onto a blog post entity.
    /// </summary>
    /// <param name="post">The blog post entity being created or updated.</param>
    /// <param name="dto">The validated request data from the caller.</param>
    private static void ApplyChanges(BlogPost post, UpsertBlogPostDto dto)
    {
        // Keep all assignable fields here. It makes create and update easier to audit,
        // especially when new editor fields are added later.
        post.Title = dto.Title;
        post.Slug = dto.Slug;
        post.Excerpt = dto.Excerpt;

        // The frontend owns the rich editor and sends both HTML and plain text.
        // HTML is used for display; plain text is useful for previews/search later.
        post.ContentHtml = dto.ContentHtml;
        post.ContentText = dto.ContentText;

        // TopicId is the managed grouping field for blog posts. It stays optional so drafts can be saved early.
        post.TopicId = dto.TopicId;

        // Tags are attached through ITagService so the same tag rows can be reused across content.

        // The API stores only the final URL. Actual file uploads are handled by the frontend.
        post.CoverImageUrl = dto.CoverImageUrl;

        // These fields control how the post appears on the public profile.
        post.SortOrder = dto.SortOrder;
        post.IsFeatured = dto.IsFeatured;
        post.IsPublished = dto.IsPublished;

        // SEO fields are optional overrides for public pages.
        post.SeoTitle = dto.SeoTitle;
        post.SeoDescription = dto.SeoDescription;
    }
}









