using System.ComponentModel.DataAnnotations;

namespace PortfolioPlatform.Api.Dtos.Auth;

/// <summary>
/// Request used to verify a one-time password.
/// </summary>
public class VerifyOtpDto
{
    /// <summary>
    /// Six-digit code submitted by the user.
    /// </summary>
    [Required]
    [MinLength(6)]
    [MaxLength(6)]
    public required string Otp { get; set; }

    /// <summary>
    /// Email address the code was originally issued for.
    /// </summary>
    [Required]
    [EmailAddress]
    public required string Email { get; set; }
}
