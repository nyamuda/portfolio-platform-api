namespace PortfolioPlatform.Api.Models.OAuth;

/// <summary>
/// Base settings required by an OAuth provider.
/// </summary>
/// <remarks>
/// Provider-specific option classes can inherit from this type when they use the same standard OAuth fields.
/// </remarks>
public class OAuthProviderOptions
{
    /// <summary>
    /// OAuth client id issued by the provider.
    /// </summary>
    public required string ClientId { get; set; }

    /// <summary>
    /// OAuth client secret issued by the provider.
    /// </summary>
    public required string ClientSecret { get; set; }

    /// <summary>
    /// Redirect URL used after a successful sign-in OAuth flow.
    /// </summary>
    public required string SigninRedirectUrl { get; set; }

    /// <summary>
    /// Redirect URL used after a successful sign-up OAuth flow.
    /// </summary>
    public required string SignupRedirectUrl { get; set; }
}
