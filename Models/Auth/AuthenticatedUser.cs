using PortfolioPlatform.Api.Enums;

namespace PortfolioPlatform.Api.Models.Auth;

/// <summary>
/// Authenticated user details extracted from a validated access or refresh token.
/// </summary>
/// <remarks>
/// Controllers use this compact payload after token validation so they do not need to parse JWT claims directly.
/// </remarks>
public class AuthenticatedUser
{
    /// <summary>
    /// User id stored in the token claims.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Username stored in the token claims.
    /// </summary>
    public required string Username { get; set; }

    /// <summary>
    /// Email address stored in the token claims.
    /// </summary>
    public required string Email { get; set; }

    /// <summary>
    /// Role stored in the token claims for authorization decisions.
    /// </summary>
    public UserRole Role { get; set; }
}
