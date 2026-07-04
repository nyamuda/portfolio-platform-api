using System.ComponentModel.DataAnnotations;

namespace PortfolioPlatform.Api.Dtos.Auth;

/// <summary>
/// Request used to complete a password reset after OTP verification.
/// </summary>
public class ResetPasswordDto
{
    /// <summary>
    /// Short-lived token issued after a valid password reset OTP is confirmed.
    /// </summary>
    [Required]
    public required string ResetToken { get; set; }

    /// <summary>
    /// New password to store for the account.
    /// </summary>
    [Required]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$")]
    public required string Password { get; set; }
}
