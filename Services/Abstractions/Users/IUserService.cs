using PortfolioPlatform.Api.Dtos.Users;

namespace PortfolioPlatform.Api.Services.Abstractions.Users;

/// <summary>
/// Provides account and public-profile operations for users.
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Retrieves account details for a user by id.
    /// </summary>
    /// <param name="id">The user id.</param>
    /// <returns>User details including account-owned fields such as email.</returns>
    Task<UserDto> GetByIdAsync(int id);

    /// <summary>
    /// Retrieves public profile information for a user by id.
    /// </summary>
    /// <param name="userId">The user id.</param>
    /// <returns>Public user details that are safe to show to visitors.</returns>
    Task<UserDto> GetPublicProfileAsync(int userId);

    /// <summary>
    /// Updates account-level public profile fields such as name and bio.
    /// </summary>
    /// <param name="userId">The authenticated user id.</param>
    /// <param name="dto">The updated profile fields.</param>
    Task UpdateProfileAsync(int userId, UpdateUserProfileDto dto);

    /// <summary>
    /// Updates a user's username after checking that the new username is available.
    /// </summary>
    /// <param name="userId">The authenticated user id.</param>
    /// <param name="dto">The requested username.</param>
    Task UpdateUsernameAsync(int userId, UpdateUsernameDto dto);
}
