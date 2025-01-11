using Microsoft.EntityFrameworkCore;
using UrlShortener.Repository;
using UrlShortener.Services;

namespace UrlShortener.Middleware;

internal static class ServiceAndRepoMiddleware
{
    internal static IServiceCollection AddServiceAndRepositoryConfiguration(this IServiceCollection services, string connectionString)
    {
        // Configure DbContext
        services.AddDbContext<PgDbContextcs>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<UrlShortenerRepository>();
        services.AddScoped<UrlShortenerService>();
        services.AddScoped<JwtService>();

        return services;
    }
}
