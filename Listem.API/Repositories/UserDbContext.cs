using Listem.API.Contracts;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Listem.API.Repositories;

public class UserDbContext(DbContextOptions<UserDbContext> options)
    : IdentityDbContext<ListemUser>(options) { }
