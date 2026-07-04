using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortfolioPlatform.Api.Dtos.Users;
using PortfolioPlatform.Api.Exceptions;
using PortfolioPlatform.Api.Models;
using PortfolioPlatform.Api.Services.Abstractions.Auth;
using PortfolioPlatform.Api.Services.Abstractions.Users;

namespace PortfolioPlatform.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController(IUserService userService, IJwtService jwtService) : ControllerBase
{
    private readonly IUserService _userService = userService;
    private readonly IJwtService _jwtService = jwtService;

    /// <summary>
    /// Retrieves account details for the specified user.
    /// </summary>
    /// <param name="id">The id of the user to retrieve.</param>
    [HttpGet("{id}", Name = "GetUserById")]
    [Authorize]
    public async Task<IActionResult> Get(int id)
    {
        try
        {
            string token = HttpContext
                .Request
                .Headers
                .Authorization
                .ToString()
                .Replace("Bearer ", "");

            int userId = _jwtService.ValidateTokenAndExtractUser(token).Id;

            if (userId != id)
                return StatusCode(403, ErrorResponse.Forbidden());

            var user = await _userService.GetByIdAsync(id);

            return Ok(user);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ErrorResponse.Create(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ErrorResponse.Create(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ErrorResponse.Unexpected(details: ex.Message));
        }
    }

    /// <summary>
    /// Retrieves public profile information for a user.
    /// </summary>
    /// <param name="userId">The id of the user whose public profile is requested.</param>
    [HttpGet("profile/{userId}", Name = "GetUserProfile")]
    public async Task<IActionResult> GetProfile(int userId)
    {
        try
        {
            var user = await _userService.GetPublicProfileAsync(userId);
            return Ok(user);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ErrorResponse.Create(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ErrorResponse.Unexpected(details: ex.Message));
        }
    }

    /// <summary>
    /// Updates the authenticated user's public profile fields.
    /// </summary>
    /// <param name="dto">The updated profile values.</param>
    [HttpPut("profile")]
    [Authorize]
    public async Task<IActionResult> UpdateProfile(UpdateUserProfileDto dto)
    {
        try
        {
            string token = HttpContext
                .Request
                .Headers
                .Authorization
                .ToString()
                .Replace("Bearer ", "");

            int userId = _jwtService.ValidateTokenAndExtractUser(token).Id;

            await _userService.UpdateProfileAsync(userId, dto);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ErrorResponse.Create(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ErrorResponse.Create(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ErrorResponse.Unexpected(details: ex.Message));
        }
    }

    /// <summary>
    /// Updates the authenticated user's username.
    /// </summary>
    /// <param name="dto">The requested username.</param>
    [HttpPatch("username")]
    [Authorize]
    public async Task<IActionResult> UpdateUsername(UpdateUsernameDto dto)
    {
        try
        {
            string token = HttpContext
                .Request
                .Headers
                .Authorization
                .ToString()
                .Replace("Bearer ", "");

            int userId = _jwtService.ValidateTokenAndExtractUser(token).Id;

            await _userService.UpdateUsernameAsync(userId, dto);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ErrorResponse.Create(ex.Message));
        }
        catch (ConflictException ex)
        {
            return StatusCode(409, ErrorResponse.Create(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ErrorResponse.Create(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ErrorResponse.Unexpected(details: ex.Message));
        }
    }
}
