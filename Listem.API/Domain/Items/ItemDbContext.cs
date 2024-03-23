using Microsoft.EntityFrameworkCore;

namespace Listem.API.Domain.Items;

internal class ItemDbContext(DbContextOptions<ItemDbContext> options) : DbContext(options)
{
    internal DbSet<Item> Items { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        // TODO: Add indices and review data structure in the schema (e.g. dates as text)
        base.OnModelCreating(builder);
        builder.HasDefaultSchema("items");
    }
}
