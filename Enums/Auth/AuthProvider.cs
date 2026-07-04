namespace PortfolioPlatform.Api.Enums.Auth;

/// <summary>
/// Represents the sign-in method used when an account was created.
/// </summary>
public enum AuthProvider
{
    /// <summary>
    /// Account created with an email address and password.
    /// </summary>
    Local,

    /// <summary>
    /// Account created through Google OAuth.
    /// </summary>
    Google,

    /// <summary>
    /// Account created through Facebook OAuth.
    /// </summary>
    Facebook,

    /// <summary>
    /// Account created through another external provider.
    /// </summary>
    Other
}
