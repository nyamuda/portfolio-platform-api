using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using PortfolioPlatform.Api.Data;
using PortfolioPlatform.Api.Dtos.Auth;
using PortfolioPlatform.Api.Dtos.Users;
using PortfolioPlatform.Api.Enums.Auth;
using PortfolioPlatform.Api.Exceptions;
using PortfolioPlatform.Api.Models;
using PortfolioPlatform.Api.Models.Users;
using PortfolioPlatform.Api.Services.Abstractions.Auth;
using PortfolioPlatform.Api.Services.Abstractions.Email;

namespace PortfolioPlatform.Api.Services.Implementations.Auth;

public class AuthService(
    ApplicationDbContext context,
    IJwtService jwtService,
    IOtpService otpService,
    IEmailService emailService,
    IEmailTemplateBuilder emailTemplateBuilder,
    ILogger<AuthService> logger
) : IAuthService
{
    protected readonly ApplicationDbContext _context = context;
    protected readonly IJwtService _jwtService = jwtService;
    private readonly IOtpService _otpService = otpService;
    private readonly IEmailService _emailService = emailService;
    private readonly IEmailTemplateBuilder _emailTemplateBuilder = emailTemplateBuilder;
    private readonly ILogger<AuthService> _logger = logger;

    /// <inheritdoc/>
    public async Task<UserDto> RegisterAsync(RegisterDto registerDto)
    {
        // Check whether another account already uses the submitted email address.
        bool userExists = await _context.Users.AnyAsync(u => u.Email.Equals(registerDto.Email));
        if (userExists)
        {
            _logger.LogWarning(
                "Registration failed: user with email {Email} already exists.",
                registerDto.Email
            );

            throw new ConflictException(
                "An account with this email already exists. Try signing in or use a different email to register."
            );
        }

        // Keep username creation in one place so OAuth registration can reuse the same rule.
        string uniqueUsername = await GenerateUniqueUsernameAsync(registerDto.Username);

        // Store only the password hash. The raw password must never be saved.
        string hashedPassword = BCrypt.Net.BCrypt.HashPassword(registerDto.Password);

        User user = new()
        {
            Username = uniqueUsername,
            Email = registerDto.Email,
            Password = hashedPassword,
            IsVerified = false,
            AuthProvider = AuthProvider.Local,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Send the verification code after saving the user so registration is persisted first.
        await RequestVerificationEmailAsync(new EmailVerificationRequestDto { Email = registerDto.Email });

        _logger.LogInformation(
            "Registration successful: added new user with email {Email}.",
            registerDto.Email
        );

        return UserDto.MapFrom(user);
    }

    /// <inheritdoc/>
    public async Task<(string accessToken, string refreshToken)> LoginAsync(LoginDto loginDto)
    {
        const double accessTokenLifespan = 4320;
        const double refreshTokenLifespan = 10080;

        // Fetch the account by email first, then verify the submitted password hash.
        User user = await _context.Users.FirstOrDefaultAsync(u => u.Email == loginDto.Email)
            ?? throw new UnauthorizedAccessException("Invalid email or password.");

        if (user.AuthProvider != AuthProvider.Local)
        {
            throw new UnauthorizedAccessException(
                "This account uses a different sign-in method. Please use that method to continue."
            );
        }

        if (string.IsNullOrWhiteSpace(user.Password))
            throw new UnauthorizedAccessException("Invalid email or password.");

        bool isCorrectPassword = BCrypt.Net.BCrypt.Verify(loginDto.Password, user.Password);
        if (!isCorrectPassword)
        {
            _logger.LogWarning(
                "Login failed: invalid credentials for user with email {Email}.",
                loginDto.Email
            );

            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        // Issue separate tokens so the browser can keep a longer-lived refresh cookie.
        string accessToken = _jwtService.GenerateJwtToken(user, expiresInMinutes: accessTokenLifespan);
        string refreshToken = _jwtService.GenerateJwtToken(user, expiresInMinutes: refreshTokenLifespan);

        _logger.LogInformation(
            "Login successful: user with email {Email} is now logged in.",
            loginDto.Email
        );

        return (accessToken, refreshToken);
    }

    /// <inheritdoc/>
    public async Task RequestPasswordResetAsync(string email)
    {
        User? existingUser = await _context
            .Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email.Equals(email));

        // Avoid revealing whether an email address is registered.
        if (existingUser is null)
        {
            _logger.LogWarning(
                "Password reset request ignored: user with email {Email} does not exist.",
                email
            );

            return;
        }

        bool isEligible = await CanRequestOtpAsync(
            userId: existingUser.Id,
            email: existingUser.Email,
            purpose: OtpPurpose.PasswordReset,
            maxRequestsPerUser: 6,
            maxRequestsPerEmail: 6
        );

        if (!isEligible)
        {
            throw new InvalidOperationException(
                "You've recently requested a password reset code. Please wait a few minutes before requesting another."
            );
        }

        string resetOtp = _otpService.Generate();
        string hashedOtp = BCrypt.Net.BCrypt.HashPassword(resetOtp);

        UserOtp userOtp = new()
        {
            Purpose = OtpPurpose.PasswordReset,
            Email = existingUser.Email,
            UserId = existingUser.Id,
            Otp = hashedOtp,
            ExpirationTime = DateTime.UtcNow.AddMinutes(10)
        };

        _context.UserOtps.Add(userOtp);
        await _context.SaveChangesAsync();

        string emailTemplate = _emailTemplateBuilder.BuildPasswordResetRequestTemplate(
            recipientName: existingUser.Username,
            otp: resetOtp
        );

        EmailMessage emailMessage = new()
        {
            RecipientName = existingUser.Username,
            RecipientEmail = existingUser.Email,
            Subject = "Password Reset Request",
            HtmlBody = emailTemplate
        };

        await _emailService.SendAsync(emailMessage);

        _logger.LogInformation("Successfully sent a password reset OTP to email {Email}", email);
    }

    /// <inheritdoc/>
    public async Task<string> VerifyOtpAndGenerateResetTokenAsync(VerifyOtpDto dto)
    {
        // Verify the OTP before issuing a short-lived password reset token.
        await _otpService.VerifyAsync(dto);

        User user = await _context.Users.FirstOrDefaultAsync(u => u.Email.Equals(dto.Email))
            ?? throw new KeyNotFoundException(
                $@"Password reset OTP verification failed. User with email ""{dto.Email}"" does not exist."
            );

        return _jwtService.GenerateJwtToken(user, expiresInMinutes: 15);
    }

    /// <inheritdoc/>
    public async Task ResetPasswordAsync(ResetPasswordDto dto)
    {
        int userId = _jwtService.ValidateTokenAndExtractUser(dto.ResetToken).Id;

        User existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Id.Equals(userId))
            ?? throw new InvalidOperationException(
                "Unable to reset password: no user found for the provided reset token."
            );

        existingUser.Password = BCrypt.Net.BCrypt.HashPassword(dto.Password);
        existingUser.AuthProvider = AuthProvider.Local;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Password successfully reset for user: {UserId}", userId);
    }

    /// <inheritdoc/>
    public async Task RequestVerificationEmailAsync(EmailVerificationRequestDto dto)
    {
        User? existingUser = await _context
            .Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email.ToLower() == dto.Email.ToLower());

        // Avoid failing hard when a verification request is made for an unknown email.
        if (existingUser is null)
        {
            _logger.LogWarning(
                "Email verification request ignored: user with email {Email} does not exist.",
                dto.Email
            );

            return;
        }

        if (existingUser.IsVerified)
        {
            _logger.LogWarning("Verification not required: email {Email} is already confirmed.", dto.Email);
            return;
        }

        bool isEligible = await CanRequestOtpAsync(
            userId: existingUser.Id,
            email: existingUser.Email,
            purpose: OtpPurpose.EmailVerification,
            maxRequestsPerUser: 5,
            maxRequestsPerEmail: 5
        );

        if (!isEligible)
        {
            throw new InvalidOperationException(
                "You've recently requested a verification code. Please wait a few minutes before requesting another."
            );
        }

        string verificationOtp = _otpService.Generate();
        string hashedOtp = BCrypt.Net.BCrypt.HashPassword(verificationOtp);

        UserOtp userOtp = new()
        {
            Purpose = OtpPurpose.EmailVerification,
            Email = dto.Email,
            UserId = existingUser.Id,
            Otp = hashedOtp,
            ExpirationTime = DateTime.UtcNow.AddMinutes(10)
        };

        _context.UserOtps.Add(userOtp);
        await _context.SaveChangesAsync();

        string emailTemplate = _emailTemplateBuilder.BuildEmailVerificationRequestTemplate(
            recipientName: existingUser.Username,
            otp: verificationOtp
        );

        EmailMessage emailMessage = new()
        {
            RecipientName = existingUser.Username,
            RecipientEmail = dto.Email,
            Subject = "Email Confirmation",
            HtmlBody = emailTemplate
        };

        await _emailService.SendAsync(emailMessage);

        _logger.LogInformation(
            "Successfully sent an email verification OTP to email {Email}",
            dto.Email
        );
    }

    /// <inheritdoc/>
    public async Task VerifyEmailAsync(VerifyOtpDto verifyOtpDto)
    {
        await _otpService.VerifyAsync(verifyOtpDto);

        User user = await _context
            .Users
            .FirstOrDefaultAsync(u => u.Email.ToLower() == verifyOtpDto.Email.ToLower())
            ?? throw new KeyNotFoundException(
                $"Email verification failed: no user found with email '{verifyOtpDto.Email}'."
            );

        user.IsVerified = true;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Email verification succeeded for {Email}", verifyOtpDto.Email);
    }

    /// <inheritdoc/>
    public async Task<string> RefreshTokenAsync(int userId)
    {
        User user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId)
            ?? throw new KeyNotFoundException($@"User with ID ""{userId}"" does not exist.");

        return _jwtService.GenerateJwtToken(user, expiresInMinutes: 4320);
    }

    /// <inheritdoc/>
    public async Task<string> GenerateUniqueUsernameAsync(string username, int maxAttempts = 10)
    {
        string candidate = username.Trim();
        int attempt = 0;

        while (await _context.Users.AnyAsync(u => u.Username == candidate) && attempt < maxAttempts)
        {
            int randomValue = RandomNumberGenerator.GetInt32(0, 1_000_000);
            candidate = $"{username}{randomValue}";
            attempt++;
        }

        if (await _context.Users.AnyAsync(u => u.Username == candidate))
        {
            _logger.LogWarning(
                "Unable to generate a unique username after {MaxAttempts} attempts. Last attempted: {Username}",
                maxAttempts,
                candidate
            );
            throw new InvalidOperationException("Unable to generate a unique username.");
        }

        return candidate;
    }

    /// <inheritdoc/>
    public async Task<bool> CanRequestOtpAsync(
        int userId,
        string email,
        OtpPurpose purpose,
        int maxRequestsPerUser = 3,
        int maxRequestsPerEmail = 3
    )
    {
        DateTime windowStart = DateTime.UtcNow.AddHours(-1);

        int userRequestCount = await _context
            .UserOtps
            .Where(o => o.UserId == userId && o.Purpose == purpose && o.CreatedAt >= windowStart)
            .CountAsync();

        if (userRequestCount >= maxRequestsPerUser)
            return false;

        int emailRequestCount = await _context
            .UserOtps
            .Where(o => o.Email == email && o.Purpose == purpose && o.CreatedAt >= windowStart)
            .CountAsync();

        return emailRequestCount < maxRequestsPerEmail;
    }
}
