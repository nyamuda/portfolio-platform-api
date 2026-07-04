namespace PortfolioPlatform.Api.Dtos.Auth.OAuth;

/// <summary>
/// Request sent after Google redirects back with an authorization code for sign-in.
/// </summary>
/// <param name="Code">Authorization code returned by Google.</param>
public record OAuthSignInDto(string Code);
