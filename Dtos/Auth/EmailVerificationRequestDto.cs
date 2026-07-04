using System.ComponentModel.DataAnnotations;

namespace PortfolioPlatform.Api.Dtos.Auth;

/// <summary>
/// Request used to send an email verification code.
/// </summary>
public class EmailVerificationRequestDto
{
    /// <summary>
    /// Email address that should receive the verification code.
    /// </summary>
    [Required]
    [EmailAddress]
    public required string Email { get; set; }
}
