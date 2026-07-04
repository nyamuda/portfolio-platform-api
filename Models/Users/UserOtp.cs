using PortfolioPlatform.Api.Enums.Auth;
using PortfolioPlatform.Api.Models.Users;

namespace PortfolioPlatform.Api.Models.Users;

/// <summary>
/// Stores a hashed one-time password issued for a sensitive account flow.
/// </summary>
/// <remarks>
/// OTP records are short-lived security records. The API stores only the hashed code so a database leak
/// does not expose usable verification or reset codes.
/// </remarks>
public class UserOtp
{
    /// <summary>
    /// Internal primary key for the OTP record.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Reason this code was issued, such as email verification or password reset.
    /// </summary>
    public required OtpPurpose Purpose { get; set; }

    /// <summary>
    /// Email address that received the code.
    /// </summary>
    /// <remarks>
    /// This is stored on the OTP record so verification can be checked against the same address that
    /// originally requested the code, even if the user later changes account details.
    /// </remarks>
    public required string Email { get; set; }

    /// <summary>
    /// Foreign key of the user account that requested the code.
    /// </summary>
    public required int UserId { get; set; }

    /// <summary>
    /// User account that owns this OTP request.
    /// </summary>
    public User? User { get; set; }

    /// <summary>
    /// BCrypt hash of the OTP value.
    /// </summary>
    /// <remarks>
    /// The raw numeric or alphanumeric code is sent by email and discarded immediately after hashing.
    /// </remarks>
    public required string Otp { get; set; }

    /// <summary>
    /// Date and time in UTC after which the code can no longer be used.
    /// </summary>
    public required DateTime ExpirationTime { get; set; }

    /// <summary>
    /// Indicates whether the code has already been used successfully.
    /// </summary>
    public bool IsUsed { get; set; } = false;

    /// <summary>
    /// Date and time when the OTP record was created in UTC.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
