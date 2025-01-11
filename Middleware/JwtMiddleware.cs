using UrlShortener.Entities;
using UrlShortener.Services;

namespace UrlShortener.Middleware;

internal class JwtMiddleware(RequestDelegate next, IConfiguration configuration)
{
    private readonly RequestDelegate _next = next;
    private readonly IConfiguration _configuration = configuration;

    public async Task InvokeAsync(HttpContext context)
    {
        var jwtService = context.RequestServices.GetService<JwtService>();

        var token = context.Request.Cookies["token"];
        var refreshToken = context.Request.Cookies["refresh_token"];

        if (!string.IsNullOrWhiteSpace(token))
        {
            var principal = jwtService.GetTokenPrincipal(token);

            if (jwtService.IsPrincipalExpiringSoon(principal))
            {
                var newToken = jwtService.GetToken();
                SetCookie(context, newToken);
                context.Request.Headers.Append("Authorization", $"Bearer {newToken.Token.Token}");
            }
            else
            {
                context.Request.Headers.Append("Authorization", $"Bearer {token}");
            }
        }

        await _next(context);
    }

    static void SetCookie(HttpContext httpContext, JwtTokenRefreshToken token)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = false,
            SameSite = SameSiteMode.Lax,
            Expires = DateTime.UtcNow.AddDays(30)
        };

        httpContext.Response.Cookies.Append("token", token.Token.Token, cookieOptions);
        httpContext.Response.Cookies.Append("refresh_token", token.RefreshToken.Token, cookieOptions);
    }

}
