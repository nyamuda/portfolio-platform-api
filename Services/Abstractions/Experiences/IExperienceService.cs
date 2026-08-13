using PortfolioPlatform.Api.Dtos.Experiences;
using PortfolioPlatform.Api.Models;

namespace PortfolioPlatform.Api.Services.Abstractions.Experiences;

/// <summary>
/// Handles timeline experience operations for profile owners and public visitors.
/// </summary>
public interface IExperienceService
{
    /// <summary>
    /// Gets all experiences owned by the authenticated user's profile.
    /// </summary>
    /// <param name="userId">Authenticated user id.</param>
    /// <param name="filters">Experience list filters from the query string.</param>
    /// <returns>A paginated page of experiences owned by the user's profile.</returns>
    Task<PageInfo<ExperienceDto>> GetMineAsync(int userId, ExperienceFilters filters);

    /// <summary>
    /// Gets one experience owned by the authenticated user's profile.
    /// </summary>
    /// <param name="userId">Authenticated user id.</param>
    /// <param name="experienceId">Experience id.</param>
    /// <returns>The requested experience.</returns>
    Task<ExperienceDto> GetMineByIdAsync(int userId, int experienceId);

    /// <summary>
    /// Gets published experiences for a public profile.
    /// </summary>
    /// <param name="profileSlug">Public profile slug.</param>
    /// <returns>Published experiences for the profile.</returns>
    Task<List<ExperienceDto>> GetPublicByProfileSlugAsync(string profileSlug);

    /// <summary>
    /// Creates an experience for the authenticated user's profile.
    /// </summary>
    /// <param name="userId">Authenticated user id.</param>
    /// <param name="dto">Experience values to create.</param>
    /// <returns>The created experience.</returns>
    Task<ExperienceDto> CreateAsync(int userId, UpsertExperienceDto dto);

    /// <summary>
    /// Updates an experience owned by the authenticated user's profile.
    /// </summary>
    /// <param name="userId">Authenticated user id.</param>
    /// <param name="experienceId">Experience id.</param>
    /// <param name="dto">Experience values to save.</param>
    /// <returns>The updated experience.</returns>
    Task<ExperienceDto> UpdateAsync(int userId, int experienceId, UpsertExperienceDto dto);

    /// <summary>
    /// Deletes an experience owned by the authenticated user's profile.
    /// </summary>
    /// <param name="userId">Authenticated user id.</param>
    /// <param name="experienceId">Experience id.</param>
    Task DeleteAsync(int userId, int experienceId);
}
