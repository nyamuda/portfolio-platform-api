using System.ComponentModel.DataAnnotations;

namespace PortfolioPlatform.Api.Dtos.Auth;

/// <summary>
/// Data required to sign in with an email address and password.
/// </summary>
public class LoginDto
{
    /// <summary>
    /// Email address connected to the account.
    /// </summary>
    [Required]
    [EmailAddress]
    [StringLength(256)]
    public required string Email { get; set; }

    /// <summary>
    /// Account password.
    /// </summary>
    [Required]
    [RegularExpression(
        @"^(?=.*[A-Za-z])(?=.*\d)(?=.*[^A-Za-z\d]).{8,}$",
        ErrorMessage = "Password must be at least 8 characters long and contain a mix of letters, numbers, and special characters"
    )]
    public required string Password { get; set; }
}
