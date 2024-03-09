using Listem.API.Contracts;
using Listem.API.Domain.Categories;
using Listem.API.Domain.Items;
using Listem.API.Domain.Lists;
using Listem.API.Endpoints;
using Listem.API.Middleware;
using Listem.API.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.OpenApi.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
var connectionString = configuration.GetConnectionString("OnDiskDatabase");

builder.Host.UseSerilog((context, config) => config.ReadFrom.Configuration(context.Configuration));
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
    options.UseSqlite(
        connectionString,
        o => o.MigrationsHistoryTable(HistoryRepository.DefaultTableName, "users")
    )
);
builder.Services.AddDbContext<ListDbContext>(options =>
    options.UseSqlite(
        connectionString,
        o => o.MigrationsHistoryTable(HistoryRepository.DefaultTableName, "lists")
    )
);
builder.Services.AddDbContext<CategoryDbContext>(options =>
    options.UseSqlite(
        connectionString,
        o => o.MigrationsHistoryTable(HistoryRepository.DefaultTableName, "categories")
    )
);
builder.Services.AddDbContext<ItemDbContext>(options =>
    options.UseSqlite(
        connectionString,
        o => o.MigrationsHistoryTable(HistoryRepository.DefaultTableName, "items")
    )
);

builder.Services.AddAuthentication().AddBearerToken(IdentityConstants.BearerScheme);
builder.Services.AddAuthorizationBuilder();
builder
    .Services.AddIdentityCore<ListemUser>()
    .AddEntityFrameworkStores<UserDbContext>()
    .AddApiEndpoints();

builder.Services.AddScoped<IRequestContext, RequestContext>();
builder.Services.AddScoped<IListService, ListService>();
builder.Services.AddScoped<IListRepository, ListRepository>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IItemService, ItemService>();
builder.Services.AddScoped<IItemRepository, ItemRepository>();

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
