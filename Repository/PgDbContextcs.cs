using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;
using UrlShortener.Entities;

namespace UrlShortener.Repository;

internal class PgDbContextcs : DbContext
{
    public PgDbContextcs(DbContextOptions<PgDbContextcs> options) : base(options)
    {
        
    }

    public DbSet<ShortUrl> ShortUrls { get; set; }
}
