using PortfolioPlatform.Api.Dtos.Themes;

namespace PortfolioPlatform.Api.Services.Abstractions.Themes;

/// <summary>
/// Reads and updates the small set of presentation choices stored on a profile.
/// </summary>
public interface IPortfolioThemeService
{
    /// <summary>Returns the authenticated owner's current portfolio design.</summary>
    Task<ThemeSelectionDto> GetMineAsync(int userId);

    /// <summary>Replaces the authenticated owner's portfolio design choices.</summary>
    Task<ThemeSelectionDto> UpdateMineAsync(int userId, UpdateThemeSelectionDto dto);

    /// <summary>Returns the design used by one published public profile.</summary>
    Task<ThemeSelectionDto> GetPublicThemeAsync(string profileSlug);
}
