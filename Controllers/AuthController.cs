using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortfolioPlatform.Api.Dtos.Auth;
using PortfolioPlatform.Api.Exceptions;
using PortfolioPlatform.Api.Models;
using PortfolioPlatform.Api.Services.Abstractions.Auth;
using PortfolioPlatform.Api.Services.Abstractions.Users;

namespace PortfolioPlatform.Api.Controllers;

/// <summary>
/// Handles account registration, sign-in, token refresh, password reset, and email verification.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IJwtService _jwtService;
    private readonly IUserService _userService;

    public AuthController(
        IAuthService authService,
        IJwtService jwtService,
        IUserService userService
    )
    {
        _authService = authService;
        _jwtService = jwtService;
        _userService = userService;
    }

    /// <summary>
    /// Registers a new account.
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto registerDto)
    {
        try
        {
            // The service handles username uniqueness, password hashing, and verification email delivery.
            var user = await _authService.RegisterAsync(registerDto);

            return CreatedAtRoute(
                routeName: "GetUserById",
                routeValues: new { id = user.Id },
                value: user
            );
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ErrorResponse.Create(ex.Message));
        }
        catch (ConflictException ex)
        {
            return StatusCode(409, ErrorResponse.Create(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ErrorResponse.Unexpected(details: ex.Message));
        }
    }

    /// <summary>
    /// Signs a user in and returns an access token.
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto loginDto)
    {
        try
        {
            var (accessToken, refreshToken) = await _authService.LoginAsync(loginDto);

            // The access token goes to the client response. The refresh token is kept
            // in an HTTP-only cookie so frontend scripts cannot read it directly.
            HttpContext.Response.Cookies.Append("refreshToken", refreshToken, BuildRefreshCookie());

            return Ok(new { token = accessToken });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ErrorResponse.Create(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ErrorResponse.Unexpected(details: ex.Message));
        }
    }

    /// <summary>
    /// Clears the refresh-token cookie.
    /// </summary>
    [HttpPost("logout")]
    public IActionResult Logout()
    {
        try
        {
            var cookieOptions = BuildRefreshCookie();

            // Overwrite the refresh token with an expired cookie so the browser removes it.
            cookieOptions.Expires = DateTime.UtcNow.AddDays(-2);

            HttpContext.Response.Cookies.Append("refreshToken", string.Empty, cookieOptions);

            return Ok();
        }
        catch (Exception ex)
        {
            return StatusCode(500, ErrorResponse.Unexpected(details: ex.Message));
        }
    }

    /// <summary>
    /// Starts password reset by sending a one-time code to the account email.
    /// </summary>
    [HttpPost("password-reset/request")]
    public async Task<IActionResult> RequestPasswordReset(PasswordResetRequestDto dto)
    {
        try
        {
            // This intentionally returns NoContent even when the email does not exist,
            // so callers cannot use the endpoint to discover registered addresses.
            await _authService.RequestPasswordResetAsync(dto.Email);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ErrorResponse.Create(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ErrorResponse.Unexpected(details: ex.Message));
        }
    }

    /// <summary>
    /// Verifies a password reset OTP and returns a short-lived reset token.
    /// </summary>
    [HttpPost("password-reset/verify-otp")]
    public async Task<IActionResult> VerifyPasswordResetOtp(VerifyOtpDto dto)
    {
        try
        {
            // The OTP itself is not enough to reset a password. A short-lived reset
            // token keeps the final password update endpoint stateless.
            string token = await _authService.VerifyOtpAndGenerateResetTokenAsync(dto);
            return Ok(new { resetToken = token });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ErrorResponse.Create(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ErrorResponse.Create(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ErrorResponse.Create(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ErrorResponse.Unexpected(details: ex.Message));
        }
    }

    /// <summary>
    /// Resets the password after a password reset OTP has been verified.
    /// </summary>
    [HttpPost("password-reset/reset")]
    public async Task<IActionResult> ResetPassword(ResetPasswordDto dto)
    {
        try
        {
            await _authService.ResetPasswordAsync(dto);
            return Ok();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ErrorResponse.Create(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ErrorResponse.Create(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ErrorResponse.Unexpected(details: ex.Message));
        }
    }

    /// <summary>
    /// Sends an email verification code.
    /// </summary>
    [HttpPost("email-verification/request")]
    public async Task<IActionResult> EmailVerificationRequest(EmailVerificationRequestDto dto)
    {
        try
        {
            // Registration calls this automatically, but exposing it separately lets
            // the frontend resend a code when the user did not receive the first one.
            await _authService.RequestVerificationEmailAsync(dto);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ErrorResponse.Create(ex.Message));
        }
        catch (ConflictException ex)
        {
            return StatusCode(409, ErrorResponse.Create(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ErrorResponse.Unexpected(details: ex.Message));
        }
    }

    /// <summary>
    /// Verifies an account email address with a one-time code.
    /// </summary>
    [HttpPost("email-verification/verify")]
    public async Task<IActionResult> VerifyEmail(VerifyOtpDto dto)
    {
        try
        {
            await _authService.VerifyEmailAsync(dto);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ErrorResponse.Create(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ErrorResponse.Create(ex.Message));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ErrorResponse.Create(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ErrorResponse.Unexpected(details: ex.Message));
        }
    }

    /// <summary>
    /// Uses a valid refresh-token cookie to issue a new access token.
    /// </summary>
    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken()
    {
        try
        {
            // Refresh tokens are read from cookies rather than the request body to match login/OAuth behavior.
            var refreshToken = HttpContext.Request.Cookies["refreshToken"];
            if (string.IsNullOrEmpty(refreshToken))
                throw new UnauthorizedAccessException(
                    "Access denied: refresh token is missing from the request."
                );

            // Validate the refresh token before issuing a new access token for the same account.
            int userId = _jwtService.ValidateTokenAndExtractUser(refreshToken).Id;
            string token = await _authService.RefreshTokenAsync(userId);

            return Ok(new { token });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ErrorResponse.Create(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ErrorResponse.Create(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ErrorResponse.Unexpected(details: ex.Message));
        }
    }

    /// <summary>
    /// Returns the authenticated user's account details.
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetAuthenticatedUser()
    {
        try
        {
            // Read the bearer token directly so the same JWT service owns token parsing everywhere.
            string token = HttpContext
                .Request
                .Headers
                .Authorization
                .ToString()
                .Replace("Bearer ", "");

            int userId = _jwtService.ValidateTokenAndExtractUser(token).Id;
            var user = await _userService.GetByIdAsync(userId);

            return Ok(user);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ErrorResponse.Create(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ErrorResponse.Create(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ErrorResponse.Unexpected(details: ex.Message));
        }
    }

    /// <summary>
    /// Builds the shared cookie settings used when writing or clearing refresh tokens.
    /// </summary>
    /// <returns>Cookie options for the refresh-token cookie.</returns>
    private static CookieOptions BuildRefreshCookie()
    {
        // SameSite=None is required when the frontend and API are on different origins.
        return new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None
        };
    }
}


