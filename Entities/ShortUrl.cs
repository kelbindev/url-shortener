using System.ComponentModel.DataAnnotations.Schema;

namespace UrlShortener.Entities;
[Table("urlshortener_shorturls")]
internal class ShortUrl()
{
    [Column("id")]
    public int Id { get; set; }
    [Column("url")]
    public string Url { get; set; } = string.Empty;
}