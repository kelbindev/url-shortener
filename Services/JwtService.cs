using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using UrlShortener.Entities;

namespace UrlShortener.Services;

internal class JwtService(IConfiguration configuration)
{
    private readonly IConfiguration _configuration = configuration;

     internal JwtTokenRefreshToken GetToken()
    {
        var token = GenerateToken(false);
        var refreshToken = GenerateRefreshToken();

        return new JwtTokenRefreshToken(token, refreshToken);
    }

    internal JwtTokenRefreshToken GetTokenFromRefreshToken(string refreshToken)
    {
        var principal = GetTokenPrincipal(refreshToken);
        if (principal is not null)
        {
            return GetToken();
        }

        return new JwtTokenRefreshToken(null,null);
    }

    private JwtToken GenerateToken(bool isRefreshToken)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT_SECRET"]));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        var tokenExpiry =
            isRefreshToken ? DateTime.UtcNow
            : DateTime.UtcNow.AddMinutes(15);

        var token = new JwtSecurityToken(
        issuer: _configuration["JWT_ISSUER"],
            audience: _configuration["JWT_AUDIENCE"],
            claims: claims,
            expires: tokenExpiry,
            signingCredentials: credentials
            );

        string jwtToken = new JwtSecurityTokenHandler().WriteToken(token);

        return new JwtToken(jwtToken, tokenExpiry);
    }

    private JwtToken GenerateRefreshToken()
    {
        var token = GenerateToken(true);
        DateTime refreshTokenExpiry = DateTime.UtcNow.AddDays(30);

        return new JwtToken(token.Token, refreshTokenExpiry);
    }

    internal ClaimsPrincipal GetTokenPrincipal(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT_SECRET"])),
            ValidateIssuer = true,
            ValidIssuer = _configuration["JWT_ISSUER"],
            ValidateAudience = true,
            ValidAudience = _configuration["JWT_AUDIENCE"],
            // Set ValidateLifetime to false to ignore token expiry
            ValidateLifetime = false
        };

        try
        {
            return tokenHandler.ValidateToken(token, validationParameters, out SecurityToken securityToken);
        }
        catch (SecurityTokenException)
        {
            return null; //not a valid jwt based on token validation parameters
        }
    }

    internal bool IsPrincipalExpiringSoon(ClaimsPrincipal principal)
    {
        var expClaim = principal.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Exp);

        if (expClaim is null)
            return false;

        var expUnixSeconds = long.Parse(expClaim.Value);
        var expDateTime = DateTimeOffset.FromUnixTimeSeconds(expUnixSeconds).DateTime;

        var timeRemaining = expDateTime - DateTime.UtcNow;
        return timeRemaining < TimeSpan.FromMinutes(5);
    }
}
