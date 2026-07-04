using Microsoft.AspNetCore.Mvc;
using PortfolioPlatform.Api.Dtos.Auth.OAuth;
using PortfolioPlatform.Api.Enums.Auth.OAuth;
using PortfolioPlatform.Api.Exceptions;
using PortfolioPlatform.Api.Models;
using PortfolioPlatform.Api.Services.Abstractions.Auth;
using PortfolioPlatform.Api.Services.Abstractions.Auth.OAuth;

namespace PortfolioPlatform.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OAuthController(IGoogleOAuthService googleOAuthService, IAuthService authService)
    : ControllerBase
{
    private readonly IGoogleOAuthService _googleOAuthService = googleOAuthService;
    private readonly IAuthService _authService = authService;

    /// <summary>
    /// Signs in an existing user with Google OAuth.
    /// </summary>
    [HttpPost("google/signin")]
    public async Task<IActionResult> GoogleSignIn(OAuthSignInDto dto)
    {
        try
        {
            // The frontend sends the short-lived provider code. The API exchanges it
            // server-side so the Google client secret never has to live in the browser.
            string googleAccessToken = await _googleOAuthService.ExchangeCodeForAccessTokenAsync(
                authorizationCode: dto.Code,
                flow: OAuthFlow.SignIn
            );

            // After Google confirms the user, continue with normal application tokens.
            var profileDetails = await _googleOAuthService.GetUserProfileAsync(googleAccessToken);
            var (accessToken, refreshToken) = await _googleOAuthService.SignInAsync(profileDetails);

            // Store the refresh token in an HTTP-only cookie to keep it away from frontend scripts.
            HttpContext.Response.Cookies.Append("refreshToken", refreshToken, BuildRefreshCookie());

            return Ok(new { token = accessToken });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ErrorResponse.Create(ex.Message));
        }
        catch (UserNotRegisteredException ex)
        {
            return NotFound(ErrorResponse.Create(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ErrorResponse.Unexpected(details: ex.ToString()));
        }
    }

    /// <summary>
    /// Registers and signs in a new user with Google OAuth.
    /// </summary>
    [HttpPost("google/signup")]
    public async Task<IActionResult> GoogleSignUp(OAuthSignUpDto dto)
    {
        try
        {
            // Sign-up has its own redirect URI so the frontend can collect a username first.
            string googleAccessToken = await _googleOAuthService.ExchangeCodeForAccessTokenAsync(
                authorizationCode: dto.Code,
                flow: OAuthFlow.SignUp
            );

            var profileDetails = await _googleOAuthService.GetUserProfileAsync(googleAccessToken);

            // Run the same username uniqueness rule used by local registration.
            string uniqueUsername = await _authService.GenerateUniqueUsernameAsync(dto.Username);
            var (accessToken, refreshToken) = await _googleOAuthService.SignUpAsync(
                profile: profileDetails,
                username: uniqueUsername
            );

            // OAuth sign-up logs the user in immediately after the account is created.
            HttpContext.Response.Cookies.Append("refreshToken", refreshToken, BuildRefreshCookie());

            return Ok(new { token = accessToken });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ErrorResponse.Create(ex.Message));
        }
        catch (ConflictException ex)
        {
            return StatusCode(409, ErrorResponse.Create(message: ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ErrorResponse.Unexpected(details: ex.ToString()));
        }
    }

    private static CookieOptions BuildRefreshCookie()
    {
        // Match the local auth cookie settings so OAuth and password flows behave consistently.
        return new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None
        };
    }
}
