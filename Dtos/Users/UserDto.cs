using PortfolioPlatform.Api.Enums;
using PortfolioPlatform.Api.Models.Users;

namespace PortfolioPlatform.Api.Dtos.Users;

/// <summary>
/// Represents account information returned by user and authentication endpoints.
/// </summary>
public class UserDto
{
    /// <summary>
    /// Unique identifier of the user.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Optional display name shown on public pages.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Unique username used for sign-in and ownership of public content.
    /// </summary>
    public required string Username { get; set; }

    /// <summary>
    /// Registered email address. Only return this in account-owned contexts.
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Optional short biography for public profile display.
    /// </summary>
    public string? Bio { get; set; }

    /// <summary>
    /// Role assigned to the user.
    /// </summary>
    public UserRole? Role { get; set; }

    /// <summary>
    /// Indicates whether the account email has been verified.
    /// </summary>
    public bool? IsVerified { get; set; }

    /// <summary>
    /// Date and time when the account was created.
    /// </summary>
    public DateTime? CreatedAt { get; set; }

    /// <summary>
    /// Maps a user entity to a user DTO without exposing password data.
    /// </summary>
    public static UserDto MapFrom(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Name = user.Name,
            Username = user.Username,
            Email = user.Email,
            Bio = user.Bio,
            Role = user.Role,
            IsVerified = user.IsVerified,
            CreatedAt = user.CreatedAt
        };
    }
}
