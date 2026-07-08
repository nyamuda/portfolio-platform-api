using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using PortfolioPlatform.Api.Enums;
using PortfolioPlatform.Api.Enums.Auth;
using PortfolioPlatform.Api.Models.Profiles;

namespace PortfolioPlatform.Api.Models.Users;

/// <summary>
/// Represents an account that can sign in, own a public profile, and manage profile content.
/// </summary>
/// <remarks>
/// This model is intentionally focused on account ownership and authentication. Public-facing profile
/// content lives on <see cref="Profile"/> so account data and public profile data can evolve separately.
/// </remarks>
[Index(nameof(Username), IsUnique = true)]
[Index(nameof(Email), IsUnique = true)]
public class User
{
    /// <summary>
    /// Internal primary key used by relationships and authorization checks.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Unique username used for sign-in and account-level identification.
    /// </summary>
    /// <remarks>
    /// Public profile URLs use <see cref="Profile.Slug"/> instead. Keeping username and profile slug
    /// separate lets a user change their public profile route without changing sign-in identity.
    /// </remarks>
    [StringLength(80)]
    public required string Username { get; set; }

    /// <summary>
    /// Unique email address used for sign-in, verification, password resets, and account messages.
    /// </summary>
    [StringLength(256)]
    public required string Email { get; set; }

    /// <summary>
    /// New email address waiting for verification before it replaces <see cref="Email" />.
    /// </summary>
    /// <remarks>
    /// Email changes are staged here so a user cannot accidentally lose access to the
    /// current sign-in email before proving they own the new address.
    /// </remarks>
    [StringLength(256)]
    public string? PendingEmail { get; set; }

    /// <summary>
    /// BCrypt hash of the local account password.
    /// </summary>
    /// <remarks>
    /// OAuth-only accounts may not have a local password until they explicitly set one later.
    /// The raw password must never be stored here.
    /// </remarks>
    public string? Password { get; set; }

    /// <summary>
    /// Optional account display name used in account surfaces and as a starting point for profile setup.
    /// </summary>
    [StringLength(160)]
    public string? Name { get; set; }

    /// <summary>
    /// Optional short account biography.
    /// </summary>
    /// <remarks>
    /// Rich public biography content belongs on <see cref="Profile.Bio"/>. This field is kept lightweight
    /// for account-level contexts where the full public profile may not be needed.
    /// </remarks>
    [StringLength(1000)]
    public string? Bio { get; set; }

    /// <summary>
    /// Provider used when the account was created or most recently linked for authentication.
    /// </summary>
    public AuthProvider? AuthProvider { get; set; } = PortfolioPlatform.Api.Enums.Auth.AuthProvider.Local;

    /// <summary>
    /// Permission level used by authorization checks.
    /// </summary>
    public UserRole Role { get; set; } = UserRole.User;

    /// <summary>
    /// Indicates whether the account email address has passed verification.
    /// </summary>
    public bool IsVerified { get; set; }

    /// <summary>
    /// Date and time when the account was created in UTC.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date and time when account-level editable fields were last updated in UTC.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Public profile owned by this account, when the user has created one.
    /// </summary>
    public Profile? Profile { get; set; }

    /// <summary>
    /// One-time password records requested by this account for verification or password reset flows.
    /// </summary>
    public List<UserOtp> UserOtps { get; set; } = [];
}

