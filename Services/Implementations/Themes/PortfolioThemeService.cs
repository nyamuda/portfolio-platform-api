using Microsoft.EntityFrameworkCore;
using PortfolioPlatform.Api.Data;
using PortfolioPlatform.Api.Dtos.Themes;
using PortfolioPlatform.Api.Models.Profiles;
using PortfolioPlatform.Api.Services.Abstractions.Themes;

namespace PortfolioPlatform.Api.Services.Implementations.Themes;

/// <summary>
/// Stores portfolio presentation choices directly on the profile they affect.
/// </summary>
public class PortfolioThemeService(ApplicationDbContext context) : IPortfolioThemeService
{
    private readonly ApplicationDbContext _context = context;

    /// <inheritdoc />
    public async Task<ThemeSelectionDto> GetMineAsync(int userId)
    {
        Profile profile = await GetOwnerProfileAsync(userId);
        return MapSelection(profile);
    }

    /// <inheritdoc />
    public async Task<ThemeSelectionDto> UpdateMineAsync(int userId, UpdateThemeSelectionDto dto)
    {
        Profile profile = await GetOwnerProfileAsync(userId);

        // Keep these values together. They describe one visual selection and should be saved
        // atomically so a public request never receives a partially updated design.
        profile.Theme = dto.Theme;
        profile.ThemeAccentColor = dto.AccentColor.ToUpperInvariant();
        profile.ThemeTypography = dto.Typography;
        profile.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return MapSelection(profile);
    }

    /// <inheritdoc />
    public async Task<ThemeSelectionDto> GetPublicThemeAsync(string profileSlug)
    {
        string normalizedSlug = profileSlug.Trim().ToLowerInvariant();
        Profile profile = await _context.Profiles.AsNoTracking().FirstOrDefaultAsync(profile =>
            profile.Slug == normalizedSlug && profile.IsPublished
        ) ?? throw new KeyNotFoundException("The public profile was not found.");

        return MapSelection(profile);
    }

    /// <summary>Loads the profile owned by the authenticated account.</summary>
    private async Task<Profile> GetOwnerProfileAsync(int userId)
    {
        return await _context.Profiles.FirstOrDefaultAsync(profile => profile.UserId == userId)
            ?? throw new KeyNotFoundException("Create your public profile before choosing a theme.");
    }

    /// <summary>Maps persisted profile fields to the small public theme contract.</summary>
    private static ThemeSelectionDto MapSelection(Profile profile) => new()
    {
        Theme = profile.Theme,
        AccentColor = profile.ThemeAccentColor,
        Typography = profile.ThemeTypography
    };
}
