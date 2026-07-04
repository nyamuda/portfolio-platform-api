using PortfolioPlatform.Api.Dtos.Projects;

namespace PortfolioPlatform.Api.Services.Abstractions.Projects;

/// <summary>
/// Handles project operations for profile owners and public visitors.
/// </summary>
public interface IProjectService
{
    /// <summary>
    /// Gets all projects owned by the authenticated user's profile.
    /// </summary>
    /// <param name="userId">Authenticated user id.</param>
    /// <returns>Projects owned by the user's profile.</returns>
    Task<List<ProjectDto>> GetMineAsync(int userId);

    /// <summary>
    /// Gets one project owned by the authenticated user's profile.
    /// </summary>
    /// <param name="userId">Authenticated user id.</param>
    /// <param name="projectId">Project id.</param>
    /// <returns>The requested project.</returns>
    Task<ProjectDto> GetMineByIdAsync(int userId, int projectId);

    /// <summary>
    /// Gets published projects for a public profile.
    /// </summary>
    /// <param name="profileSlug">Public profile slug.</param>
    /// <returns>Published projects for the profile.</returns>
    Task<List<ProjectDto>> GetPublicByProfileSlugAsync(string profileSlug);

    /// <summary>
    /// Gets one published project by profile slug and project slug.
    /// </summary>
    /// <param name="profileSlug">Public profile slug.</param>
    /// <param name="projectSlug">Public project slug.</param>
    /// <returns>The published project.</returns>
    Task<ProjectDto> GetPublicBySlugAsync(string profileSlug, string projectSlug);

    /// <summary>
    /// Creates a project for the authenticated user's profile.
    /// </summary>
    /// <param name="userId">Authenticated user id.</param>
    /// <param name="dto">Project values to create.</param>
    /// <returns>The created project.</returns>
    Task<ProjectDto> CreateAsync(int userId, UpsertProjectDto dto);

    /// <summary>
    /// Updates a project owned by the authenticated user's profile.
    /// </summary>
    /// <param name="userId">Authenticated user id.</param>
    /// <param name="projectId">Project id.</param>
    /// <param name="dto">Project values to save.</param>
    /// <returns>The updated project.</returns>
    Task<ProjectDto> UpdateAsync(int userId, int projectId, UpsertProjectDto dto);

    /// <summary>
    /// Deletes a project owned by the authenticated user's profile.
    /// </summary>
    /// <param name="userId">Authenticated user id.</param>
    /// <param name="projectId">Project id.</param>
    Task DeleteAsync(int userId, int projectId);
}
