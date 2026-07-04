using Microsoft.EntityFrameworkCore;
using PortfolioPlatform.Api.Data;
using PortfolioPlatform.Api.Dtos.BlogPosts;
using PortfolioPlatform.Api.Exceptions;
using PortfolioPlatform.Api.Models.Content;
using PortfolioPlatform.Api.Services.Abstractions.BlogPosts;

namespace PortfolioPlatform.Api.Services.Implementations.BlogPosts;

/// <summary>
/// Handles blog post management for profile owners and public visitors.
/// </summary>
public class BlogPostService(ApplicationDbContext context) : IBlogPostService
{
    private readonly ApplicationDbContext _context = context;

    /// <inheritdoc/>
    public async Task<List<BlogPostDto>> GetMineAsync(int userId)
    {
        // Blog posts belong to profiles, not directly to users. This lookup also proves ownership.
        int profileId = await GetOwnedProfileIdAsync(userId);

        // Owner reads include drafts because the editor and dashboard need to show unfinished posts.
        // Public endpoints apply stricter filters separately, so drafts do not leak to visitors.
        return await BlogPostDtos(
                _context
                    .BlogPosts
                    .AsNoTracking()
                    .Where(post => post.ProfileId == profileId)
            )
            // Featured posts should float to the top in owner views so they are easy to manage.
            .OrderByDescending(post => post.IsFeatured)
            // SortOrder gives the owner manual control when several posts are featured or pinned.
            .ThenBy(post => post.SortOrder)
            // Newer posts should appear first when the manual ordering is the same.
            .ThenByDescending(post => post.CreatedAt)
            .ToListAsync();
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
            .FirstOrDefaultAsync(post => post.Id == postId && post.ProfileId == profileId)
            ?? throw new KeyNotFoundException($"Blog post with ID '{postId}' was not found.");

        // The current post is excluded from this check so a user can save without changing the slug.
        await EnsureSlugIsAvailableAsync(profileId, dto.Slug, postId);

        // Keep all editable field assignments together so we do not forget fields between create and update.
        ApplyChanges(post, dto);
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
            Category = post.Category,
            Tags = post.Tags,
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

        // Category and tags are lightweight organization fields. They are intentionally optional.
        post.Category = dto.Category;
        post.Tags = dto.Tags;

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

