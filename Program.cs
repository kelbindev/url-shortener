using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using UrlShortener.Repository;
using UrlShortener.Services;

var builder = WebApplication.CreateBuilder(args);

// Setup AZURE_CLIENT_ID, AZURE_CLIENT_SECRET, and AZURE_TENANT_ID in your environment variable
var keyVaultUrl = builder.Configuration["AZURE_KEYVAULT_URL"];
var client = new SecretClient(vaultUri: new Uri(keyVaultUrl), credential: new DefaultAzureCredential());
var secret = client.GetSecret("heroku-postgresql-research");

var connectionString = secret.Value.Value;

// Configure DbContext
builder.Services.AddDbContext<PgDbContextcs>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped<UrlShortenerRepository>();
builder.Services.AddScoped<UrlShortenerService>();

await using var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// Home page: A form for submitting a URL
app.MapGet("/", ctx =>
                {
                    ctx.Response.ContentType = "text/html";
                    return ctx.Response.SendFileAsync("index.html");
                });

// API endpoint for shortening a URL and save it to a local database
app.MapPost("/url", ShortenerDelegate);

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