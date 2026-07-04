using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PortfolioPlatform.Api.Data;
using PortfolioPlatform.Api.Enums.Auth;
using PortfolioPlatform.Api.Enums.Auth.OAuth;
using PortfolioPlatform.Api.Exceptions;
using PortfolioPlatform.Api.Models;
using PortfolioPlatform.Api.Models.OAuth;
using PortfolioPlatform.Api.Models.Users;
using PortfolioPlatform.Api.Services.Abstractions.Auth;
using PortfolioPlatform.Api.Services.Abstractions.Auth.OAuth;
using RestSharp;

namespace PortfolioPlatform.Api.Services.Implementations.Auth.OAuth;

public class GoogleOAuthService(
    ApplicationDbContext context,
    IOptions<GoogleOAuthOptions> options,
    IJwtService jwtService
) : IGoogleOAuthService
{
    private readonly ApplicationDbContext _context = context;
    private readonly GoogleOAuthOptions _options = options.Value;
    private readonly IJwtService _jwtService = jwtService;

    private const double AccessTokenLifespan = 4320;
    private const double RefreshTokenLifespan = 10080;

    /// <inheritdoc/>
    public async Task<string> ExchangeCodeForAccessTokenAsync(
        string authorizationCode,
        OAuthFlow flow
    )
    {
        var restClient = new RestClient("https://oauth2.googleapis.com/token");

        // Google validates the redirect URI against the one used in the browser flow.
        // Keep sign-in and sign-up redirect URIs separate so the frontend can handle
        // each flow with the right screen and error messages.
        string redirectUri = flow switch
        {
            OAuthFlow.SignIn => _options.SigninRedirectUrl,
            OAuthFlow.SignUp => _options.SignupRedirectUrl,
            _ => throw new ArgumentOutOfRangeException(nameof(flow))
        };

        // The token endpoint expects form-url-encoded values rather than JSON.
        // RestSharp handles the encoding while keeping the request readable.
        var request = new RestRequest()
            .AddHeader("Content-Type", "application/x-www-form-urlencoded")
            .AddParameter("client_id", _options.ClientId)
            .AddParameter("client_secret", _options.ClientSecret)
            .AddParameter("code", authorizationCode)
            .AddParameter("redirect_uri", redirectUri)
            .AddParameter("grant_type", "authorization_code");

        request.Method = Method.Post;

        var response = await restClient.ExecuteAsync<GoogleOAuthTokenResponse>(request);
        if (!response.IsSuccessful)
        {
            string body = response.Content ?? "<empty>";
            string errorMessage = response.ErrorMessage ?? "<no error message>";

            // Keep the provider response in the exception so local debugging can show
            // whether the failure is a bad code, wrong redirect URI, or bad client secret.
            throw new Exception(
                $"Error getting Google OAuth token: Status {(int)response.StatusCode}; Body: {body}; Error: {errorMessage}"
            );
        }

        return response.Data?.AccessToken
            ?? throw new InvalidOperationException("The Google OAuth response did not include an access token.");
    }

    /// <inheritdoc/>
    public async Task<OAuthUserProfile> GetUserProfileAsync(string accessToken)
    {
        var client = new RestClient("https://www.googleapis.com/oauth2/v3/userinfo");

        // Use the provider access token only to fetch the identity details we need.
        // The application still issues its own JWTs for API access after this step.
        var request = new RestRequest()
            .AddHeader("Accept", "application/json")
            .AddHeader("Authorization", $"Bearer {accessToken}");

        var response = await client.ExecuteAsync<OAuthUserProfile>(request);
        if (!response.IsSuccessful)
        {
            throw new Exception($"Error getting Google user details: {response.ErrorMessage}");
        }

        return response.Data ?? throw new InvalidOperationException("The response data was null.");
    }

    /// <inheritdoc/>
    public async Task<(string accessToken, string refreshToken)> SignInAsync(OAuthUserProfile profile)
    {
        // OAuth sign-in is only for accounts that already exist locally.
        // New OAuth users must go through the sign-up endpoint so username rules still run.
        User user = await _context.Users.FirstOrDefaultAsync(u => u.Email == profile.Email)
            ?? throw new UserNotRegisteredException(
                "No account found for this email. Please sign up to continue."
            );

        // Do not silently merge OAuth and password accounts. Users must sign in with
        // the method they used when the account was created.
        if (user.AuthProvider != AuthProvider.Google)
        {
            throw new InvalidOperationException(
                "This email is already registered with another method. Please sign in using your email and password instead of Google."
            );
        }

        // Once the provider identity has been checked, issue normal application JWTs.
        string accessToken = _jwtService.GenerateJwtToken(user, expiresInMinutes: AccessTokenLifespan);
        string refreshToken = _jwtService.GenerateJwtToken(user, expiresInMinutes: RefreshTokenLifespan);

        return (accessToken, refreshToken);
    }

    /// <inheritdoc/>
    public async Task<(string accessToken, string refreshToken)> SignUpAsync(
        OAuthUserProfile profile,
        string username
    )
    {
        // Email remains the account-level unique identity even when the username is separate.
        User? existingUserByEmail = await _context.Users.FirstOrDefaultAsync(u => u.Email == profile.Email);
        if (existingUserByEmail is not null)
        {
            throw new ConflictException(
                $"An account with email '{profile.Email}' already exists. Please log in instead."
            );
        }

        // Google has already verified the email, so no separate email OTP is required here.
        User newUser = new()
        {
            Name = profile.Name,
            Email = profile.Email,
            Username = username,
            IsVerified = true,
            AuthProvider = AuthProvider.Google,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(newUser);
        await _context.SaveChangesAsync();

        // Return tokens immediately so OAuth sign-up behaves like local registration + login.
        string accessToken = _jwtService.GenerateJwtToken(newUser, expiresInMinutes: AccessTokenLifespan);
        string refreshToken = _jwtService.GenerateJwtToken(newUser, expiresInMinutes: RefreshTokenLifespan);

        return (accessToken, refreshToken);
    }
}
