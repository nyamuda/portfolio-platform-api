using PortfolioPlatform.Api.Dtos.Profiles;

namespace PortfolioPlatform.Api.Services.Abstractions.Profiles;

/// <summary>
/// Handles owner and public profile operations.
/// </summary>
public interface IProfileService
{
    /// <summary>
    /// Gets the authenticated user's profile, if one exists.
    /// </summary>
    /// <param name="userId">Authenticated user id.</param>
    /// <returns>The user's profile, or null when no profile exists.</returns>
    Task<ProfileDto?> GetMineAsync(int userId);

    /// <summary>
    /// Gets a published public profile by slug.
    /// </summary>
    /// <param name="slug">Public profile slug.</param>
    /// <returns>The published profile.</returns>
    Task<ProfileDto> GetPublicBySlugAsync(string slug);

    /// <summary>
    /// Creates or updates the authenticated user's profile.
    /// </summary>
    /// <param name="userId">Authenticated user id.</param>
    /// <param name="dto">Profile values to save.</param>
    /// <returns>The saved profile.</returns>
    Task<ProfileDto> UpsertAsync(int userId, UpsertProfileDto dto);

    /// <summary>
    /// Deletes the authenticated user's profile and profile-owned content.
    /// </summary>
    /// <param name="userId">Authenticated user id.</param>
    Task DeleteMineAsync(int userId);
}

