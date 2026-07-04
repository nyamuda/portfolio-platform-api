using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using PortfolioPlatform.Api.Data;
using PortfolioPlatform.Api.Dtos.Auth;
using PortfolioPlatform.Api.Services.Abstractions.Auth;

namespace PortfolioPlatform.Api.Services.Implementations.Auth;

public class OtpService(ApplicationDbContext context, ILogger<OtpService> logger) : IOtpService
{
    private readonly ApplicationDbContext _context = context;
    private readonly ILogger<OtpService> _logger = logger;

    /// <inheritdoc/>
    public string Generate()
    {
        // Generate a secure random integer between 0 and 999999.
        int randomNumber = RandomNumberGenerator.GetInt32(0, 1_000_000);

        // Always return exactly six digits, including leading zeroes.
        return randomNumber.ToString("D6");
    }

    /// <inheritdoc/>
    public async Task VerifyAsync(VerifyOtpDto dto)
    {
        // Only the newest active unused code is valid. Older codes remain in the table
        // for audit/rate-limit history but should not be accepted once a newer one exists.
        var userOtp = await _context
            .UserOtps
            .Where(
                userOtp =>
                    userOtp.Email.Equals(dto.Email)
                    && userOtp.ExpirationTime > DateTime.UtcNow
                    && !userOtp.IsUsed
            )
            .OrderByDescending(userOtp => userOtp.CreatedAt)
            .FirstOrDefaultAsync()
            ?? throw new InvalidOperationException(
                "Your code has expired or is invalid. Please request a new one."
            );

        bool isOtpCorrect = BCrypt.Net.BCrypt.Verify(dto.Otp, userOtp.Otp);
        if (isOtpCorrect)
        {
            userOtp.IsUsed = true;
            await _context.SaveChangesAsync();

            _logger.LogInformation("OTP verification successful for {Email}", dto.Email);
            return;
        }

        _logger.LogWarning("OTP verification failed for {Email}", dto.Email);
        throw new UnauthorizedAccessException(
            "We couldn't verify your OTP. Double-check the code and try again."
        );
    }
}
