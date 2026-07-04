using System.ComponentModel.DataAnnotations;

namespace PortfolioPlatform.Api.Dtos.Users;

/// <summary>
/// Data used when a user changes their username.
/// </summary>
public class UpdateUsernameDto
{
    /// <summary>
    /// New unique username.
    /// </summary>
    [Required]
    [StringLength(80, MinimumLength = 3)]
    [RegularExpression(
        @"^[A-Za-z0-9._-]+$",
        ErrorMessage = "Username can only contain letters, numbers, dots, underscores, and hyphens"
    )]
    public required string Username { get; set; }
}
