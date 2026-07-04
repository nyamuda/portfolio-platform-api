namespace PortfolioPlatform.Api.Models.OAuth;

/// <summary>
/// Google OAuth settings loaded from application configuration.
/// </summary>
/// <remarks>
/// This class exists even though it currently adds no new fields, because it keeps Google-specific
/// configuration explicit and leaves room for provider-specific settings later.
/// </remarks>
public class GoogleOAuthOptions : OAuthProviderOptions;
