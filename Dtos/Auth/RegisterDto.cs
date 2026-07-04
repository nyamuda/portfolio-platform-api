using System.ComponentModel.DataAnnotations;

namespace PortfolioPlatform.Api.Dtos.Auth;

/// <summary>
/// Data required to create a new account.
/// </summary>
public class RegisterDto
{
    /// <summary>
    /// Public username used for sign-in and profile ownership.
    /// </summary>
    [Required]
    [StringLength(80, MinimumLength = 3)]
    [RegularExpression(
        @"^[A-Za-z0-9._-]+$",
        ErrorMessage = "Username can only contain letters, numbers, dots, underscores, and hyphens"
    )]
    public required string Username { get; set; }

    /// <summary>
    /// Email address used for sign-in and account communication.
    /// </summary>
    [Required]
    [EmailAddress]
    [StringLength(256)]
    public required string Email { get; set; }

    /// <summary>
    /// Account password. Must include letters, numbers, and at least one special character.
    /// </summary>
    [Required]
    [RegularExpression(
        @"^(?=.*[A-Za-z])(?=.*\d)(?=.*[^A-Za-z\d]).{8,}$",
        ErrorMessage = "Password must be at least 8 characters long and contain a mix of letters, numbers, and special characters"
    )]
    public required string Password { get; set; }
}
