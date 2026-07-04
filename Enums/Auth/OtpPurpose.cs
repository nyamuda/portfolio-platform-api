namespace PortfolioPlatform.Api.Enums.Auth;

/// <summary>
/// Describes why a one-time password was created.
/// </summary>
public enum OtpPurpose
{
    /// <summary>
    /// Code used to verify a user's email address.
    /// </summary>
    EmailVerification,

    /// <summary>
    /// Code used before issuing a password reset token.
    /// </summary>
    PasswordReset,

    /// <summary>
    /// Code used to confirm an email change.
    /// </summary>
    EmailChange,

    /// <summary>
    /// Code used before a sensitive account action.
    /// </summary>
    SensitiveAction
}
