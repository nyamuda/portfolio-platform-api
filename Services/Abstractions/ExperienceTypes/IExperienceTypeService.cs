using PortfolioPlatform.Api.Dtos.ExperienceTypes;
using PortfolioPlatform.Api.Models;

namespace PortfolioPlatform.Api.Services.Abstractions.ExperienceTypes;

/// <summary>
/// Handles experience type lookup, listing, creation, update, and deletion.
/// </summary>
public interface IExperienceTypeService
{
    /// <summary>
    /// Retrieves a paginated list of experience types with usage counts.
    /// </summary>
    /// <param name="queryParams">Search, sorting, and pagination values for the request.</param>
    /// <returns>A paginated list of experience types.</returns>
    Task<PageInfo<ExperienceTypeDto>> GetExperienceTypesAsync(ExperienceTypeQueryParams queryParams);

    /// <summary>
    /// Creates a new experience type from administrator-managed metadata.
    /// </summary>
    /// <param name="dto">The experience type details to create.</param>
    /// <returns>The newly created experience type.</returns>
    Task<ExperienceTypeDto> CreateAsync(UpsertExperienceTypeDto dto);

    /// <summary>
    /// Updates an existing experience type and keeps its public metadata current.
    /// </summary>
    /// <param name="experienceTypeId">The experience type identifier.</param>
    /// <param name="dto">The experience type details to save.</param>
    /// <returns>The updated experience type.</returns>
    Task<ExperienceTypeDto> UpdateAsync(int experienceTypeId, UpsertExperienceTypeDto dto);

    /// <summary>
    /// Deletes an experience type when no timeline entries are using it.
    /// </summary>
    /// <param name="experienceTypeId">The experience type identifier.</param>
    Task DeleteAsync(int experienceTypeId);
}
