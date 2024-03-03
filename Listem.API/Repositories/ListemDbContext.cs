using Listem.API.Contracts;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Listem.API.Repositories;

public class ListemDbContext(DbContextOptions<ListemDbContext> options) : IdentityDbContext<ListemUser>(options);
