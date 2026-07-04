using PortfolioPlatform.Api.Models.Auth;
using PortfolioPlatform.Api.Models.Users;

namespace PortfolioPlatform.Api.Services.Abstractions.Auth;

/// <summary>
/// Provides JWT creation and validation operations for authenticated API users.
/// </summary>
public interface IJwtService
{
    /// <summary>
    /// Generates a signed JWT for the specified user.
    /// </summary>
    /// <param name="user">The user for whom the token is generated.</param>
    /// <param name="expiresInMinutes">Token lifetime in minutes.</param>
    /// <returns>A signed JWT string.</returns>
    string GenerateJwtToken(User user, double expiresInMinutes = 10);

    /// <summary>
    /// Validates a JWT and extracts the authenticated user payload stored in its claims.
    /// </summary>
    /// <param name="token">The JWT to validate.</param>
    /// <returns>The authenticated user details contained in the token.</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown when the token is missing, invalid, or expired.</exception>
    AuthenticatedUser ValidateTokenAndExtractUser(string token);
}
