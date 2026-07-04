using PortfolioPlatform.Api.Dtos.Auth;

namespace PortfolioPlatform.Api.Services.Abstractions.Auth;

/// <summary>
/// Generates and verifies one-time passwords used by authentication flows.
/// </summary>
public interface IOtpService
{
    /// <summary>
    /// Generates a cryptographically secure six-digit OTP.
    /// </summary>
    /// <returns>A six-character numeric code.</returns>
    string Generate();

    /// <summary>
    /// Verifies the latest active, unused OTP for the supplied email address.
    /// </summary>
    /// <param name="dto">Email and OTP submitted by the user.</param>
    Task VerifyAsync(VerifyOtpDto dto);
}
