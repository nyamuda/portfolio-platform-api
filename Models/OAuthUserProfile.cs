namespace PortfolioPlatform.Api.Models;

/// <summary>
/// Normalized profile details returned by an external OAuth provider.
/// </summary>
/// <param name="Name">Display name supplied by the OAuth provider.</param>
/// <param name="Email">Email address supplied by the OAuth provider.</param>
/// <remarks>
/// Provider-specific JSON is mapped into this record so the rest of the authentication flow can work
/// with one simple shape regardless of which OAuth provider was used.
/// </remarks>
public record OAuthUserProfile(string Name, string Email);
