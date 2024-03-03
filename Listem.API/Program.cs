using Listem.API.Contracts;
using Listem.API.Controllers;
using Listem.API.Repositories;
using Listem.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
var connectionString = configuration.GetConnectionString("DefaultConnection");

builder.Services.AddHttpLogging(logging =>
{
    logging.LoggingFields = HttpLoggingFields.All;
    logging.RequestHeaders.Add("sec-ch-ua");
    logging.MediaTypeOptions.AddText("application/json");
    logging.RequestBodyLogLimit = 4096;
    logging.ResponseBodyLogLimit = 4096;
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            Description = "Authorization header using the Bearer scheme",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey
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

builder.Services.AddDbContext<ListemDbContext>(options => options.UseSqlite(connectionString));

builder.Services.AddAuthentication().AddBearerToken(IdentityConstants.BearerScheme);
builder.Services.AddAuthorizationBuilder();
builder
    .Services.AddIdentityCore<ListemUser>()
    .AddEntityFrameworkStores<ListemDbContext>()
    .AddApiEndpoints();

builder.Services.AddSingleton<IDatabaseProvider, DatabaseProvider>();
builder.Services.AddSingleton<ItemListService>();
builder.Services.AddSingleton<IItemListRepository, ItemListRepository>();
builder.Services.AddSingleton<ItemListController>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapIdentityApi<ListemUser>();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.UseHttpLogging();
app.Run();
