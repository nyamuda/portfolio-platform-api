using System.Text.Json.Serialization;

namespace PortfolioPlatform.Api.Models.OAuth;

/// <summary>
/// Token response returned by Google's OAuth token endpoint.
/// </summary>
/// <remarks>
/// Property names match Google's JSON response through <see cref="JsonPropertyNameAttribute"/> so the
/// application can keep normal C# naming while still deserializing provider responses correctly.
/// </remarks>
public class GoogleOAuthTokenResponse
{
    /// <summary>
    /// Provider access token used to fetch profile details from Google.
    /// </summary>
    [JsonPropertyName("access_token")]
    public required string AccessToken { get; set; }

    /// <summary>
    /// Token lifetime in seconds as returned by Google.
    /// </summary>
    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }

    /// <summary>
    /// Space-separated provider scopes included in the token.
    /// </summary>
    [JsonPropertyName("scope")]
    public string? Scope { get; set; }

    /// <summary>
    /// Provider token type, usually Bearer.
    /// </summary>
    [JsonPropertyName("token_type")]
    public string? TokenType { get; set; }

    /// <summary>
    /// Optional OpenID Connect id token returned by Google.
    /// </summary>
    [JsonPropertyName("id_token")]
    public string? IdToken { get; set; }
}
