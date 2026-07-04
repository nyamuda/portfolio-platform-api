namespace PortfolioPlatform.Api.Enums;

/// <summary>
/// Defines the broad permission level for a user account.
/// Keep this small at first; richer permissions can be added later if teams or moderation grow.
/// </summary>
public enum UserRole
{
    /// <summary>
    /// Standard account with access to regular profile and content features.
    /// </summary>
    User = 0,

    /// <summary>
    /// Administrative account with access to management features.
    /// </summary>
    Admin = 1
}
