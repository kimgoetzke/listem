using Listem.API.Contracts;
using Listem.API.Domain.Categories;
using Listem.API.Domain.Items;
using Listem.API.Domain.Lists;
using Listem.API.Endpoints;
using Listem.API.Middleware;
using Listem.API.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
var connectionString = configuration.GetConnectionString("DefaultConnection");

builder.Services.AddProblemDetails();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Listem API", Version = "v1" });
    options.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            Description = "Authorization header using the Bearer scheme",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
            Scheme = "Bearer"
        }
    );
    options.AddSecurityRequirement(
        new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Id = "Bearer",
                        Type = ReferenceType.SecurityScheme
                    }
                },
                new List<string>()
            }
        }
    );
});

builder.Services.AddDbContext<UserDbContext>(options =>
{
    // If not using SQLite, replace with other DB provider and add the following line to the options:
    // Action<SqliteDbContextOptionsBuilder>? sqliteOptionsAction = o => o.MigrationsHistoryTable(HistoryRepository.DefaultTableName, "users");
    options.UseSqlite(connectionString);
});

builder.Services.AddAuthentication().AddBearerToken(IdentityConstants.BearerScheme);
builder.Services.AddAuthorizationBuilder();
builder
    .Services.AddIdentityCore<ListemUser>()
    .AddEntityFrameworkStores<UserDbContext>()
    .AddApiEndpoints();
builder.Services.AddScoped<IRequestContext, RequestContext>();

builder.Services.AddSingleton<IListService, ListService>();
builder.Services.AddSingleton<IListRepository, PlaceholderListRepository>();
builder.Services.AddSingleton<ICategoryService, CategoryService>();
builder.Services.AddSingleton<ICategoryRepository, PlaceholderCategoryRepository>();
builder.Services.AddSingleton<IItemService, ItemService>();
builder.Services.AddSingleton<IItemRepository, PlaceholderItemRepository>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapListEndpoints();
app.MapCategoryEndpoints();
app.MapItemEndpoints();
app.MapIdentityApi<ListemUser>().ShortCircuit();
app.UseAuthentication();
app.UseAuthorization();
app.UseRequestMiddleware();
app.UseHttpExceptionHandler();
app.MapShortCircuit(404, "robots.txt", "favicon.ico", "404.html", "sitemap.xml");

app.Run();
