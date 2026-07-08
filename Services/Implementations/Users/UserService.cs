using Microsoft.EntityFrameworkCore;
using PortfolioPlatform.Api.Data;
using PortfolioPlatform.Api.Dtos.Auth;
using PortfolioPlatform.Api.Dtos.Users;
using PortfolioPlatform.Api.Exceptions;
using PortfolioPlatform.Api.Services.Abstractions.Auth;
using PortfolioPlatform.Api.Services.Abstractions.Users;

namespace PortfolioPlatform.Api.Services.Implementations.Users;

public class UserService(ApplicationDbContext context, IAuthService authService) : IUserService
{
    private readonly ApplicationDbContext _context = context;
    private readonly IAuthService _authService = authService;

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
                PendingEmail = u.PendingEmail,
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
    public async Task UpdateEmailAsync(int userId, UpdateEmailDto dto)
    {
        // Retrieve the account first so the pending email can only be attached to
        // the authenticated user who requested the change.
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId)
            ?? throw new KeyNotFoundException($@"User with ID '{userId}' does not exist.");

        string requestedEmail = dto.Email.Trim();

        // If the submitted email is already the active email, there is nothing to verify.
        if (user.Email.Equals(requestedEmail, StringComparison.OrdinalIgnoreCase))
            return;

        // Do not allow two accounts to claim the same active or pending email address.
        bool emailAlreadyInUse = await _context
            .Users
            .AnyAsync(u =>
                u.Id != userId
                && (
                    u.Email.ToLower() == requestedEmail.ToLower()
                    || (u.PendingEmail != null && u.PendingEmail.ToLower() == requestedEmail.ToLower())
                )
            );

        if (emailAlreadyInUse)
            throw new ConflictException("An account with this email already exists.");

        // Store the new address separately until the user proves they can receive mail there.
        user.PendingEmail = requestedEmail;
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        // Reuse the same OTP email flow as normal verification so the frontend can send
        // the user to the existing email-verification screen after this request succeeds.
        await _authService.RequestVerificationEmailAsync(
            new EmailVerificationRequestDto { Email = requestedEmail }
        );
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
