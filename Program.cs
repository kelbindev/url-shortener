using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.AspNetCore.WebUtilities;
using UrlShortener.Entities;
using UrlShortener.Middleware;
using UrlShortener.Services;

var builder = WebApplication.CreateBuilder(args);

// Setup AZURE_CLIENT_ID, AZURE_CLIENT_SECRET, and AZURE_TENANT_ID in your environment variable
var keyVaultUrl = builder.Configuration["AZURE_KEYVAULT_URL"];
var client = new SecretClient(vaultUri: new Uri(keyVaultUrl), credential: new DefaultAzureCredential());
var secretConnectionString = client.GetSecret(builder.Configuration["AZURE_SECRET_NAME_CONNECTIONSTRING"]);

var connectionString = secretConnectionString.Value.Value;

//add jwt auth
builder.Services.AddJwtConfiguration(builder.Configuration);

//add services and repo
builder.Services.AddServiceAndRepositoryConfiguration(connectionString);

await using var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseMiddleware<JwtMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

// Home page: A form for submitting a URL
app.MapGet("/", ctx =>
                {
                    ctx.Response.ContentType = "text/html";
                    return ctx.Response.SendFileAsync("index.html");
                });

// API endpoint for shortening a URL and save it to a database
app.MapPost("/url", ShortenerDelegate).RequireAuthorization();
// API endpoint for getting authentication token
app.MapPost("/token", GetToken);
// API endpoint for clearing authentication token
app.MapPost("/cleartoken", ClearToken);
// Catch all page: redirecting shortened URL to its original address
app.MapFallback(RedirectDelegate);

await app.RunAsync();
return;

static async Task ShortenerDelegate(HttpContext httpContext)
{
    var request = await httpContext.Request.ReadFromJsonAsync<string>() ;

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

static Task RedirectDelegate(HttpContext httpContext)
{
    var service = httpContext.RequestServices.GetRequiredService<UrlShortenerService>();

    var path = httpContext.Request.Path.ToUriComponent().Trim('/');
    
    var su = service.GetByPath(path).Result;

    httpContext.Response.Redirect(su.Url ?? "/");

    return Task.CompletedTask;
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