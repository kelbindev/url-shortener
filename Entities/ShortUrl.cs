namespace UrlShortener.Entities;

internal class ShortUrl(Uri url)
{
    public int Id { get; protected set; }
    public string Url { get; protected set; } = url.ToString();
}