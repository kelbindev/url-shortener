using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.AspNetCore.WebUtilities;
using UrlShortener.Components;
using UrlShortener.Entities;
using UrlShortener.Middleware;
using UrlShortener.Services;

var builder = WebApplication.CreateBuilder(args);

var connectionString = string.Empty;
var keyVaultUrl = builder.Configuration["AZURE_KEYVAULT_URL"];

if (!string.IsNullOrWhiteSpace(keyVaultUrl))
{
    var client = new SecretClient(vaultUri: new Uri(keyVaultUrl), credential: new DefaultAzureCredential());
    var secretConnectionString = client.GetSecret(builder.Configuration["AZURE_SECRET_NAME_CONNECTIONSTRING"]);
    connectionString = secretConnectionString.Value.Value;
}
else
{
    connectionString = builder.Configuration.GetConnectionString("pgdb")
        ?? throw new InvalidOperationException("Connection string 'pgdb' not found.");
}

builder.Services.AddJwtConfiguration(builder.Configuration);
builder.Services.AddServiceAndRepositoryConfiguration(connectionString);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

await using var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.UseDeveloperExceptionPage();

app.UseStaticFiles();

// Resolve short URL codes before Blazor routing picks up the path.
// Filters out static assets (dots), Blazor internals (underscore prefix), and nested paths (slash).
app.Use(async (context, next) =>
{
    var path = context.Request.Path.Value?.Trim('/');
    if (!string.IsNullOrEmpty(path) && !path.Contains('/') && !path.Contains('.') && !path.StartsWith('_'))
    {
        try
        {
            var service = context.RequestServices.GetRequiredService<UrlShortenerService>();
            var su = await service.GetByPath(path);
            if (!string.IsNullOrEmpty(su.Url))
            {
                context.Response.Redirect(su.Url);
                return;
            }
        }
        catch { }
    }
    await next();
});

app.UseMiddleware<JwtMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

// Token management endpoints — must remain as HTTP endpoints to set HttpOnly cookies.
app.MapPost("/token", GetToken);
app.MapPost("/cleartoken", ClearToken);
app.MapPost("/url", ShortenerDelegate).RequireAuthorization();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

await app.RunAsync();
return;

static async Task ShortenerDelegate(HttpContext httpContext)
{
    var request = await httpContext.Request.ReadFromJsonAsync<string>();

    if (!Uri.TryCreate(request, UriKind.Absolute, out var inputUri))
    {
        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        await httpContext.Response.WriteAsync("URL is invalid.");
        return;
    }

    var service = httpContext.RequestServices.GetRequiredService<UrlShortenerService>();
    var su = await service.CreateNewShortUrl(inputUri);

    var urlChunk = WebEncoders.Base64UrlEncode(BitConverter.GetBytes(su.Id));
    var result = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}/{urlChunk}";
    await httpContext.Response.WriteAsJsonAsync(new { url = result });
}

static Task GetToken(HttpContext httpContext)
{
    var jwtService = httpContext.RequestServices.GetRequiredService<JwtService>();
    var token = jwtService.GetToken();
    SetCookie(httpContext, token);
    return Task.CompletedTask;
}

static Task ClearToken(HttpContext httpContext)
{
    httpContext.Response.Cookies.Delete("token");
    httpContext.Response.Cookies.Delete("refresh_token");
    return Task.CompletedTask;
}

static void SetCookie(HttpContext httpContext, JwtTokenRefreshToken token)
{
    var cookieOptions = new CookieOptions
    {
        HttpOnly = true,
        Secure = false,
        SameSite = SameSiteMode.Lax,
        Expires = DateTime.UtcNow.AddDays(7)
    };

    httpContext.Response.Cookies.Append("token", token.Token.Token, cookieOptions);
    httpContext.Response.Cookies.Append("refresh_token", token.RefreshToken.Token, cookieOptions);
}
