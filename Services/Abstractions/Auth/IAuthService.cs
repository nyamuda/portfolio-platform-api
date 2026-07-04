using PortfolioPlatform.Api.Dtos.Auth;
using PortfolioPlatform.Api.Dtos.Users;
using PortfolioPlatform.Api.Enums.Auth;

namespace PortfolioPlatform.Api.Services.Abstractions.Auth;

/// <summary>
/// Handles account registration, sign-in, verification, password reset, and token refresh operations.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Registers a new local account, stores the password hash, and sends an email verification code.
    /// </summary>
    /// <param name="registerDto">The account registration details.</param>
    /// <returns>The newly created user.</returns>
    Task<UserDto> RegisterAsync(RegisterDto registerDto);

    /// <summary>
    /// Signs a user in with email and password.
    /// </summary>
    /// <param name="loginDto">The login details.</param>
    /// <returns>An access token and refresh token.</returns>
    Task<(string accessToken, string refreshToken)> LoginAsync(LoginDto loginDto);

    /// <summary>
    /// Sends a password reset code to the user's email address when the account exists.
    /// </summary>
    /// <param name="email">Email address requesting a password reset.</param>
    Task RequestPasswordResetAsync(string email);

    /// <summary>
    /// Verifies a password reset OTP and returns a short-lived reset token.
    /// </summary>
    /// <param name="dto">Email and OTP submitted by the user.</param>
    /// <returns>A short-lived token that can be used to set a new password.</returns>
    Task<string> VerifyOtpAndGenerateResetTokenAsync(VerifyOtpDto dto);

    /// <summary>
    /// Resets a password after validating a password reset token.
    /// </summary>
    /// <param name="dto">Reset token and new password.</param>
    Task ResetPasswordAsync(ResetPasswordDto dto);

    /// <summary>
    /// Sends an email verification code for a registered account.
    /// </summary>
    /// <param name="dto">Email address that should receive the verification code.</param>
    Task RequestVerificationEmailAsync(EmailVerificationRequestDto dto);

    /// <summary>
    /// Verifies a user's email address using a one-time password.
    /// </summary>
    /// <param name="verifyOtpDto">Email and OTP submitted by the user.</param>
    Task VerifyEmailAsync(VerifyOtpDto verifyOtpDto);

    /// <summary>
    /// Generates a new access token for a user who has a valid refresh token.
    /// </summary>
    /// <param name="userId">The authenticated user id extracted from the refresh token.</param>
    /// <returns>A new access token.</returns>
    Task<string> RefreshTokenAsync(int userId);

    /// <summary>
    /// Generates a unique username by appending a secure random number when the requested username is already taken.
    /// </summary>
    /// <param name="username">The preferred username.</param>
    /// <param name="maxAttempts">Maximum suffix attempts before failing.</param>
    /// <returns>A unique username.</returns>
    Task<string> GenerateUniqueUsernameAsync(string username, int maxAttempts = 10);

    /// <summary>
    /// Checks whether an OTP request is allowed under the current per-user and per-email rate limits.
    /// </summary>
    /// <param name="userId">User requesting the OTP.</param>
    /// <param name="email">Email address receiving the OTP.</param>
    /// <param name="purpose">Reason for the OTP.</param>
    /// <param name="maxRequestsPerUser">Maximum requests per user in the time window.</param>
    /// <param name="maxRequestsPerEmail">Maximum requests per email in the time window.</param>
    /// <returns>True when another OTP can be requested; otherwise false.</returns>
    Task<bool> CanRequestOtpAsync(
        int userId,
        string email,
        OtpPurpose purpose,
        int maxRequestsPerUser = 3,
        int maxRequestsPerEmail = 3
    );
}
