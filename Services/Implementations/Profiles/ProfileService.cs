using Microsoft.EntityFrameworkCore;
using PortfolioPlatform.Api.Data;
using PortfolioPlatform.Api.Dtos.Profiles;
using PortfolioPlatform.Api.Exceptions;
using PortfolioPlatform.Api.Models.Profiles;
using PortfolioPlatform.Api.Services.Abstractions.Profiles;

namespace PortfolioPlatform.Api.Services.Implementations.Profiles;

public class ProfileService(ApplicationDbContext context) : IProfileService
{
    private readonly ApplicationDbContext _context = context;

    /// <inheritdoc/>
    public async Task<ProfileDto?> GetMineAsync(int userId)
    {
        // Owner reads can return drafts, so do not filter by IsPublished here.
        return await _context
            .Profiles
            .AsNoTracking()
            .Where(profile => profile.UserId == userId)
            .Select(profile => new ProfileDto
            {
                Id = profile.Id,
                UserId = profile.UserId,
                DisplayName = profile.DisplayName,
                Slug = profile.Slug,
                Headline = profile.Headline,
                Tagline = profile.Tagline,
                Bio = profile.Bio,
                AboutContentText = profile.AboutContentText,
                AboutContentHtml = profile.AboutContentHtml,
                CurrentFocus = profile.CurrentFocus,
                ProfileImageUrl = profile.ProfileImageUrl,
                CoverImageUrl = profile.CoverImageUrl,
                Location = profile.Location,
                WebsiteUrl = profile.WebsiteUrl,
                GitHubUrl = profile.GitHubUrl,
                LinkedInUrl = profile.LinkedInUrl,
                XUrl = profile.XUrl,
                SeoTitle = profile.SeoTitle,
                SeoDescription = profile.SeoDescription,
                IsPublished = profile.IsPublished,
                PublishedProjectCount = profile.Projects.Count(project => project.IsPublished),
                CreatedAt = profile.CreatedAt,
                UpdatedAt = profile.UpdatedAt
            })
            .FirstOrDefaultAsync();
    }

    /// <inheritdoc/>
    public async Task<ProfileDto> GetPublicBySlugAsync(string slug)
    {
        // Public profile reads must only return published profiles.
        ProfileDto profile = await _context
            .Profiles
            .AsNoTracking()
            .Where(profile => profile.Slug == slug && profile.IsPublished)
            .Select(profile => new ProfileDto
            {
                Id = profile.Id,
                UserId = profile.UserId,
                DisplayName = profile.DisplayName,
                Slug = profile.Slug,
                Headline = profile.Headline,
                Tagline = profile.Tagline,
                Bio = profile.Bio,
                AboutContentText = profile.AboutContentText,
                AboutContentHtml = profile.AboutContentHtml,
                CurrentFocus = profile.CurrentFocus,
                ProfileImageUrl = profile.ProfileImageUrl,
                CoverImageUrl = profile.CoverImageUrl,
                Location = profile.Location,
                WebsiteUrl = profile.WebsiteUrl,
                GitHubUrl = profile.GitHubUrl,
                LinkedInUrl = profile.LinkedInUrl,
                XUrl = profile.XUrl,
                SeoTitle = profile.SeoTitle,
                SeoDescription = profile.SeoDescription,
                IsPublished = profile.IsPublished,
                PublishedProjectCount = profile.Projects.Count(project => project.IsPublished),
                CreatedAt = profile.CreatedAt,
                UpdatedAt = profile.UpdatedAt
            })
            .FirstOrDefaultAsync()
            ?? throw new KeyNotFoundException($"Profile with slug '{slug}' was not found.");

        return profile;
    }

    /// <inheritdoc/>
    public async Task<ProfileDto> UpsertAsync(int userId, UpsertProfileDto dto)
    {
        Profile? profile = await _context.Profiles.FirstOrDefaultAsync(profile => profile.UserId == userId);

        // Slugs are public URLs, so they must be unique across every profile.
        bool slugAlreadyInUse = await _context
            .Profiles
            .AnyAsync(profile => profile.UserId != userId && profile.Slug == dto.Slug);

        if (slugAlreadyInUse)
            throw new ConflictException("This profile link is already taken.");

        if (profile is null)
        {
            // First profile setup creates the one profile owned by this account.
            profile = new Profile
            {
                UserId = userId,
                DisplayName = dto.DisplayName,
                Slug = dto.Slug,
                Headline = dto.Headline,
                CreatedAt = DateTime.UtcNow
            };

            _context.Profiles.Add(profile);
        }

        // Keep the field assignment explicit so later profile fields are easy to audit.
        profile.DisplayName = dto.DisplayName;
        profile.Slug = dto.Slug;
        profile.Headline = dto.Headline;
        profile.Tagline = dto.Tagline;
        profile.Bio = dto.Bio;
        profile.AboutContentText = dto.AboutContentText;
        profile.AboutContentHtml = dto.AboutContentHtml;
        profile.CurrentFocus = dto.CurrentFocus;
        profile.ProfileImageUrl = dto.ProfileImageUrl;
        profile.CoverImageUrl = dto.CoverImageUrl;
        profile.Location = dto.Location;
        profile.WebsiteUrl = dto.WebsiteUrl;
        profile.GitHubUrl = dto.GitHubUrl;
        profile.LinkedInUrl = dto.LinkedInUrl;
        profile.XUrl = dto.XUrl;
        profile.SeoTitle = dto.SeoTitle;
        profile.SeoDescription = dto.SeoDescription;
        profile.IsPublished = dto.IsPublished;
        profile.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return await GetMineAsync(userId)
            ?? throw new InvalidOperationException("Profile was saved but could not be loaded.");
    }

    /// <inheritdoc/>
    public async Task DeleteMineAsync(int userId)
    {
        Profile profile = await _context.Profiles.FirstOrDefaultAsync(profile => profile.UserId == userId)
            ?? throw new KeyNotFoundException("Profile was not found.");

        // Projects and blog posts are cascade deleted through the Profile relationships.
        _context.Profiles.Remove(profile);
        await _context.SaveChangesAsync();
    }
}
