using PortfolioPlatform.Api.Enums.Auth.OAuth;
using PortfolioPlatform.Api.Models;

namespace PortfolioPlatform.Api.Services.Abstractions.Auth.OAuth;

/// <summary>
/// Handles Google OAuth token exchange, profile lookup, sign-in, and sign-up.
/// </summary>
public interface IGoogleOAuthService
{
    /// <summary>
    /// Exchanges a Google authorization code for a Google access token.
    /// </summary>
    /// <param name="authorizationCode">Authorization code returned by Google.</param>
    /// <param name="flow">OAuth flow that produced the code.</param>
    /// <returns>A Google access token.</returns>
    Task<string> ExchangeCodeForAccessTokenAsync(string authorizationCode, OAuthFlow flow);

    /// <summary>
    /// Gets the Google user's profile details from the Google userinfo endpoint.
    /// </summary>
    /// <param name="accessToken">Google access token.</param>
    /// <returns>Normalized OAuth profile details.</returns>
    Task<OAuthUserProfile> GetUserProfileAsync(string accessToken);

    /// <summary>
    /// Signs in an existing Google account and returns API tokens.
    /// </summary>
    /// <param name="profile">Google profile details.</param>
    /// <returns>An access token and refresh token.</returns>
    Task<(string accessToken, string refreshToken)> SignInAsync(OAuthUserProfile profile);

    /// <summary>
    /// Creates a new Google account and returns API tokens.
    /// </summary>
    /// <param name="profile">Google profile details.</param>
    /// <param name="username">Unique username selected for the account.</param>
    /// <returns>An access token and refresh token.</returns>
    Task<(string accessToken, string refreshToken)> SignUpAsync(
        OAuthUserProfile profile,
        string username
    );
}
