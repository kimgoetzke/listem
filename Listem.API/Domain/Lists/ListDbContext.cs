using Microsoft.EntityFrameworkCore;

namespace Listem.API.Domain.Lists;

internal class ListDbContext(DbContextOptions<ListDbContext> options) : DbContext(options)
{
    internal DbSet<List> Lists { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        // TODO: Add indices and review data structure in the schema (e.g. dates as text)
        base.OnModelCreating(builder);
        builder.HasDefaultSchema("lists");
    }
}
