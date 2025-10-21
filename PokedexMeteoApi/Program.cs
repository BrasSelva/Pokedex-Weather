using System.IO;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using PokedexMeteoApi.Infrastructure;
using PokedexMeteoApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddUserSecrets<Program>(optional: true);

var useRedis = builder.Configuration.GetValue("Cache:UseRedis", true);
if (useRedis)
{
    // ex: appsettings: "Redis:Configuration": "localhost:6379"
    var redisCfg = builder.Configuration["Redis:Configuration"] ?? "localhost:6379";
    var instance = builder.Configuration["Redis:InstanceName"] ?? "pokedex:";

    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = redisCfg;
        options.InstanceName = instance;
    });
}
else
{
    builder.Services.AddDistributedMemoryCache();
}

builder.Services.AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        o.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

// ------------ Swagger ------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ------------ EF Core + SQLite (DB dans App_Data) ------------
builder.Services.AddDbContext<AppDbContext>(opt =>
{
    var dataDir = Path.Combine(builder.Environment.ContentRootPath, "App_Data");
    Directory.CreateDirectory(dataDir);
    var dbPath = Path.Combine(dataDir, "pokedex.db");
    opt.UseSqlite($"Data Source={dbPath}");
});

builder.Services.AddHttpClient<IWeatherClient, WeatherClient>();
builder.Services.AddScoped<IWeatherEffectService, WeatherEffectService>();

// ------------ CORS (ouvert pour dev) ------------
builder.Services.AddCors(opt =>
{
    opt.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

var app = builder.Build();

// ------------ Pipeline ------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();
app.MapControllers();

// Création/MAJ auto de la base au démarrage
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.Run();
