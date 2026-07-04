using System.ComponentModel.DataAnnotations;

namespace PortfolioPlatform.Api.Dtos.Users;

/// <summary>
/// Data used to update the account-level public profile fields.
/// </summary>
public class UpdateUserProfileDto
{
    /// <summary>
    /// Optional display name shown on public pages.
    /// </summary>
    [MaxLength(160)]
    public string? Name { get; set; }

    /// <summary>
    /// Optional short biography shown on public pages.
    /// </summary>
    [MaxLength(1000)]
    public string? Bio { get; set; }
}
