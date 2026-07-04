namespace PortfolioPlatform.Api.Models.Auth;

/// <summary>
/// JWT configuration bound from appsettings, user-secrets, or environment variables.
/// </summary>
/// <remarks>
/// The signing key is sensitive and should be supplied through secrets or environment configuration outside local development.
/// </remarks>
public class JwtSettings
{
    /// <summary>
    /// Token issuer value expected during JWT validation.
    /// </summary>
    public string Issuer { get; set; } = string.Empty;

    /// <summary>
    /// Token audience value expected during JWT validation.
    /// </summary>
    public string Audience { get; set; } = string.Empty;

    /// <summary>
    /// Symmetric signing key used to create and validate JWTs.
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Default access-token lifetime in minutes.
    /// </summary>
    public int ExpiresInMinutes { get; set; } = 1440;
}
