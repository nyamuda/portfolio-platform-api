using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PortfolioPlatform.Api.Enums;
using PortfolioPlatform.Api.Models.Auth;
using PortfolioPlatform.Api.Models.Users;
using PortfolioPlatform.Api.Services.Abstractions.Auth;

namespace PortfolioPlatform.Api.Services.Implementations.Auth;

public class JwtService(IOptions<JwtSettings> jwtOptions) : IJwtService
{
    private readonly JwtSettings _jwtSettings = jwtOptions.Value;

    /// <inheritdoc/>
    public string GenerateJwtToken(User user, double expiresInMinutes = 10)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        // Keep the token payload small and limited to the identity values the API needs.
        List<Claim> claims =
        [
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        ];

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiresInMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <inheritdoc/>
    public AuthenticatedUser ValidateTokenAndExtractUser(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new UnauthorizedAccessException("Access denied: token is missing.");

        var tokenHandler = new JwtSecurityTokenHandler();
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));

        ClaimsPrincipal principal;
        try
        {
            principal = tokenHandler.ValidateToken(
                token,
                new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = _jwtSettings.Issuer,
                    ValidAudience = _jwtSettings.Audience,
                    IssuerSigningKey = securityKey,
                    ClockSkew = TimeSpan.Zero
                },
                out _
            );
        }
        catch (Exception ex)
        {
            throw new UnauthorizedAccessException("Access denied: token is invalid or expired.", ex);
        }

        // Validate each required claim before returning an authenticated user payload.
        var idClaim = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        var username = principal.FindFirstValue(ClaimTypes.Name);
        var email = principal.FindFirstValue(ClaimTypes.Email);
        var roleClaim = principal.FindFirstValue(ClaimTypes.Role);

        if (!int.TryParse(idClaim, out int userId))
            throw new UnauthorizedAccessException("Access denied: token lacks a valid user id.");

        if (string.IsNullOrWhiteSpace(username))
            throw new UnauthorizedAccessException("Access denied: token lacks a valid username.");

        if (string.IsNullOrWhiteSpace(email))
            throw new UnauthorizedAccessException("Access denied: token lacks a valid email.");

        if (!Enum.TryParse(roleClaim, out UserRole role))
            throw new UnauthorizedAccessException("Access denied: token lacks a valid role.");

        return new AuthenticatedUser
        {
            Id = userId,
            Username = username,
            Email = email,
            Role = role
        };
    }
}
