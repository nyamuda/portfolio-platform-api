using Microsoft.EntityFrameworkCore;
using PortfolioPlatform.Api.Data;
using PortfolioPlatform.Api.Dtos.Users;
using PortfolioPlatform.Api.Exceptions;
using PortfolioPlatform.Api.Services.Abstractions.Users;

namespace PortfolioPlatform.Api.Services.Implementations.Users;

public class UserService(ApplicationDbContext context) : IUserService
{
    private readonly ApplicationDbContext _context = context;

    /// <inheritdoc/>
    public async Task<UserDto> GetByIdAsync(int id)
    {
        // Project only the account fields needed by the caller instead of loading the full entity graph.
        UserDto user = await _context
            .Users
            .AsNoTracking()
            .Select(u => new UserDto
            {
                Id = u.Id,
                Name = u.Name,
                Username = u.Username,
                Email = u.Email,
                Bio = u.Bio,
                Role = u.Role,
                IsVerified = u.IsVerified,
                CreatedAt = u.CreatedAt
            })
            .FirstOrDefaultAsync(u => u.Id == id)
            ?? throw new KeyNotFoundException($@"User with ID '{id}' does not exist.");

        return user;
    }

    /// <inheritdoc/>
    public async Task<UserDto> GetPublicProfileAsync(int userId)
    {
        // Public reads intentionally omit private account fields such as email and password.
        UserDto user = await _context
            .Users
            .AsNoTracking()
            .Select(u => new UserDto
            {
                Id = u.Id,
                Name = u.Name,
                Username = u.Username,
                Bio = u.Bio,
                Role = u.Role,
                IsVerified = u.IsVerified,
                CreatedAt = u.CreatedAt
            })
            .FirstOrDefaultAsync(u => u.Id == userId)
            ?? throw new KeyNotFoundException($@"User with ID '{userId}' does not exist.");

        return user;
    }

    /// <inheritdoc/>
    public async Task UpdateProfileAsync(int userId, UpdateUserProfileDto dto)
    {
        // Retrieve the account first so updates stay scoped to the authenticated user.
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId)
            ?? throw new KeyNotFoundException($@"User with ID '{userId}' does not exist.");

        user.Name = dto.Name;
        user.Bio = dto.Bio;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    /// <inheritdoc/>
    public async Task UpdateUsernameAsync(int userId, UpdateUsernameDto dto)
    {
        // Retrieve the account first so updates stay scoped to the authenticated user.
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId)
            ?? throw new KeyNotFoundException($@"User with ID '{userId}' does not exist.");

        // Check whether another account already uses the requested username.
        bool usernameAlreadyInUse = await _context
            .Users
            .AnyAsync(u => u.Id != userId && u.Username.ToLower() == dto.Username.ToLower());

        if (usernameAlreadyInUse)
            throw new ConflictException("This username is already taken.");

        user.Username = dto.Username.Trim();
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }
}
