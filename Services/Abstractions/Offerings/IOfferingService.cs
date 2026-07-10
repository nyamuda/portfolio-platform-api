using PortfolioPlatform.Api.Dtos.Offerings;
using PortfolioPlatform.Api.Models;

namespace PortfolioPlatform.Api.Services.Abstractions.Offerings;

/// <summary>
/// Handles offering operations for profile owners and public visitors.
/// </summary>
public interface IOfferingService
{
    /// <summary>
    /// Gets all offerings owned by the authenticated user's profile.
    /// </summary>
    /// <param name="userId">Authenticated user id.</param>
    /// <param name="filters">Offering list filters from the query string.</param>
    /// <returns>A paginated page of offerings owned by the user's profile.</returns>
    Task<PageInfo<OfferingDto>> GetMineAsync(int userId, OfferingFilters filters);

    /// <summary>
    /// Gets one offering owned by the authenticated user's profile.
    /// </summary>
    /// <param name="userId">Authenticated user id.</param>
    /// <param name="offeringId">Offering id.</param>
    /// <returns>The requested offering.</returns>
    Task<OfferingDto> GetMineByIdAsync(int userId, int offeringId);

    /// <summary>
    /// Gets published offerings for a public profile.
    /// </summary>
    /// <param name="profileSlug">Public profile slug.</param>
    /// <returns>Published offerings for the profile.</returns>
    Task<List<OfferingDto>> GetPublicByProfileSlugAsync(string profileSlug);

    /// <summary>
    /// Gets one published offering by profile slug and offering slug.
    /// </summary>
    /// <param name="profileSlug">Public profile slug.</param>
    /// <param name="offeringSlug">Public offering slug.</param>
    /// <returns>The published offering.</returns>
    Task<OfferingDto> GetPublicBySlugAsync(string profileSlug, string offeringSlug);

    /// <summary>
    /// Creates an offering for the authenticated user's profile.
    /// </summary>
    /// <param name="userId">Authenticated user id.</param>
    /// <param name="dto">Offering values to create.</param>
    /// <returns>The created offering.</returns>
    Task<OfferingDto> CreateAsync(int userId, UpsertOfferingDto dto);

    /// <summary>
    /// Updates an offering owned by the authenticated user's profile.
    /// </summary>
    /// <param name="userId">Authenticated user id.</param>
    /// <param name="offeringId">Offering id.</param>
    /// <param name="dto">Offering values to save.</param>
    /// <returns>The updated offering.</returns>
    Task<OfferingDto> UpdateAsync(int userId, int offeringId, UpsertOfferingDto dto);

    /// <summary>
    /// Deletes an offering owned by the authenticated user's profile.
    /// </summary>
    /// <param name="userId">Authenticated user id.</param>
    /// <param name="offeringId">Offering id.</param>
    Task DeleteAsync(int userId, int offeringId);
}
