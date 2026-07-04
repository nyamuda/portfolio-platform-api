namespace PortfolioPlatform.Api.Enums.Auth.OAuth;

/// <summary>
/// Identifies which OAuth browser flow produced an authorization code.
/// </summary>
public enum OAuthFlow
{
    /// <summary>
    /// OAuth code was created for an existing-user sign-in flow.
    /// </summary>
    SignIn,

    /// <summary>
    /// OAuth code was created for a new-user sign-up flow.
    /// </summary>
    SignUp
}
