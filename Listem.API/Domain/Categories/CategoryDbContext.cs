using Microsoft.EntityFrameworkCore;

namespace Listem.API.Domain.Categories;

internal class CategoryDbContext(DbContextOptions<CategoryDbContext> options) : DbContext(options)
{
    internal DbSet<Category> Categories { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        // TODO: Add indices and review data structure in the schema (e.g. dates as text)
        base.OnModelCreating(builder);
        builder.HasDefaultSchema("lists");
    }
}
