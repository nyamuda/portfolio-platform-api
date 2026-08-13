using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortfolioPlatform.Api.Dtos.ExperienceTypes;
using PortfolioPlatform.Api.Enums.Experiences;
using PortfolioPlatform.Api.Exceptions;
using PortfolioPlatform.Api.Models;
using PortfolioPlatform.Api.Services.Abstractions.ExperienceTypes;

namespace PortfolioPlatform.Api.Controllers;

/// <summary>
/// Handles experience type lookup for public forms and management for administrators.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class ExperienceTypesController(
    IExperienceTypeService experienceTypeService,
    ILogger<ExperienceTypesController> logger
) : ControllerBase
{
    private readonly IExperienceTypeService _experienceTypeService = experienceTypeService;
    private readonly ILogger<ExperienceTypesController> _logger = logger;

    /// <summary>
    /// Gets a paginated list of experience types.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Get(
        string? search = null,
        int page = 1,
        int pageSize = 20,
        ExperienceTypeSortOption sortBy = ExperienceTypeSortOption.Manual
    )
    {
        try
        {
            ExperienceTypeQueryParams queryParams = new()
            {
                Page = page,
                PageSize = pageSize,
                SortBy = sortBy,
                Search = search
            };

            PageInfo<ExperienceTypeDto> types = await _experienceTypeService.GetExperienceTypesAsync(queryParams);
            return Ok(types);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to load experience types.");
            return StatusCode(500, ErrorResponse.Unexpected());
        }
    }

    /// <summary>
    /// Creates an experience type that can be assigned to timeline entries.
    /// </summary>
    /// <param name="dto">The experience type details to create.</param>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(UpsertExperienceTypeDto dto)
    {
        try
        {
            ExperienceTypeDto type = await _experienceTypeService.CreateAsync(dto);
            return CreatedAtAction(nameof(Get), new { search = type.Name }, type);
        }
        catch (ConflictException exception)
        {
            return StatusCode(409, ErrorResponse.Create(exception.Message));
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to create experience type '{ExperienceTypeName}'.", dto.Name);
            return StatusCode(500, ErrorResponse.Unexpected());
        }
    }

    /// <summary>
    /// Updates administrator-managed metadata for an existing experience type.
    /// </summary>
    /// <param name="experienceTypeId">The experience type identifier.</param>
    /// <param name="dto">The experience type details to save.</param>
    [HttpPut("{experienceTypeId:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int experienceTypeId, UpsertExperienceTypeDto dto)
    {
        try
        {
            ExperienceTypeDto type = await _experienceTypeService.UpdateAsync(experienceTypeId, dto);
            return Ok(type);
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(ErrorResponse.Create(exception.Message));
        }
        catch (ConflictException exception)
        {
            return StatusCode(409, ErrorResponse.Create(exception.Message));
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to update experience type {ExperienceTypeId}.", experienceTypeId);
            return StatusCode(500, ErrorResponse.Unexpected());
        }
    }

    /// <summary>
    /// Deletes an experience type when it is not used by any timeline entries.
    /// </summary>
    /// <param name="experienceTypeId">The experience type identifier.</param>
    [HttpDelete("{experienceTypeId:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int experienceTypeId)
    {
        try
        {
            await _experienceTypeService.DeleteAsync(experienceTypeId);
            return NoContent();
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(ErrorResponse.Create(exception.Message));
        }
        catch (ConflictException exception)
        {
            return StatusCode(409, ErrorResponse.Create(exception.Message));
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to delete experience type {ExperienceTypeId}.", experienceTypeId);
            return StatusCode(500, ErrorResponse.Unexpected());
        }
    }
}
