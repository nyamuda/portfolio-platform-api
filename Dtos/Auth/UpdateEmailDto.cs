using System.ComponentModel.DataAnnotations;

namespace PortfolioPlatform.Api.Dtos.Auth;

/// <summary>
/// Data required when a user requests an email address change.
/// </summary>
public class UpdateEmailDto
{
    /// <summary>
    /// New email address that should receive a verification code before it becomes active.
    /// </summary>
    [Required]
    [EmailAddress]
    public required string Email { get; set; }
}
