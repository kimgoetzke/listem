using Listem.API.Contracts;
using Listem.API.Domain.Categories;
using Listem.API.Domain.ItemLists;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Listem.API.Repositories;

public class ListemDbContext(DbContextOptions<ListemDbContext> options) : IdentityDbContext<ListemUser>(options)
{
    public required DbSet<List> ListItems { get; set; }
    public required DbSet<Category> Categories { get; set; }
}
