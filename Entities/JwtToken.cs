namespace UrlShortener.Entities;

public record JwtToken(string Token, DateTime TokenExpiry);

public record JwtTokenRefreshToken(JwtToken Token, JwtToken RefreshToken);