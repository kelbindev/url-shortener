using UrlShortener.Entities;

namespace UrlShortener.Repository;

internal class UrlShortenerRepository(PgDbContextcs context)
{
    private readonly PgDbContextcs _context = context;

    internal async Task<ShortUrl> GetById(int id)
    {
        var su = await _context.ShortUrls.FindAsync(id);

        if (su is null) return new ShortUrl();

        return su;
    }

    internal async Task<ShortUrl> Insert(ShortUrl url)
    {
        await _context.ShortUrls.AddAsync(url);

        await _context.SaveChangesAsync();

        return url;
    }
}
