using Microsoft.AspNetCore.WebUtilities;
using UrlShortener.Entities;
using UrlShortener.Repository;

namespace UrlShortener.Services;

internal class UrlShortenerService(UrlShortenerRepository repo)
{
    private readonly UrlShortenerRepository _repo = repo;

    internal async Task<ShortUrl> GetByPath(string path)
    {
        var id = BitConverter.ToInt32(WebEncoders.Base64UrlDecode(path));

        return await _repo.GetById(id);
    }

    internal async Task<ShortUrl> CreateNewShortUrl(Uri url)
    {
        var su = new ShortUrl
        {
            Url = url.ToString()
        };

        return await _repo.Insert(su);
    }
}
