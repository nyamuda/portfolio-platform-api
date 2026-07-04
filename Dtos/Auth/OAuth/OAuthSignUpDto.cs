namespace PortfolioPlatform.Api.Dtos.Auth.OAuth;

/// <summary>
/// Request sent after Google redirects back with an authorization code for sign-up.
/// </summary>
/// <param name="Code">Authorization code returned by Google.</param>
/// <param name="Username">Preferred username for the new account.</param>
public record OAuthSignUpDto(string Code, string Username);
