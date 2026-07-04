using System.ComponentModel.DataAnnotations;

namespace PortfolioPlatform.Api.Dtos.Auth;

/// <summary>
/// Request used to start the password reset flow.
/// </summary>
public class PasswordResetRequestDto
{
    /// <summary>
    /// Email address of the account that needs a password reset code.
    /// </summary>
    [Required]
    [EmailAddress]
    public required string Email { get; set; }
}
