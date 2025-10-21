using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using System.Text;

namespace PokedexMeteoApi.Services;

public sealed class OpenWeatherClient : IWeatherClient
{
    private readonly HttpClient _http;
    private readonly IDistributedCache _cache;
    private readonly string _apiKey;
    private readonly string _baseUrl;

    public OpenWeatherClient(HttpClient http, IConfiguration cfg, IDistributedCache cache)
    {
        _http = http;
        _cache = cache;
        _apiKey = cfg["Weather:ApiKey"] ?? "";
        _baseUrl = (cfg["Weather:BaseUrl"] ?? "https://api.openweathermap.org/data/2.5").TrimEnd('/');
    }

    public async Task<Weather> GetWeatherByCityAsync(string city, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_apiKey))
            throw new InvalidOperationException("Weather API key is missing.");

        var cacheKey = $"weather:{city.ToLower()}";
        var cached = await _cache.GetStringAsync(cacheKey, ct);

        if (!string.IsNullOrEmpty(cached))
        {
            Console.WriteLine($"[CACHE] Hit pour {city}");
            return Enum.Parse<Weather>(cached);
        }

        Console.WriteLine($"[CACHE] Miss pour {city}");
        var q = Uri.EscapeDataString(city.Trim());
        var url = $"{_baseUrl}/weather?q={q}&appid={_apiKey}&units=metric&lang=fr";

        using var resp = await _http.GetAsync(url, ct);
        resp.EnsureSuccessStatusCode();

        var dto = await resp.Content.ReadFromJsonAsync<OpenWeatherResp>(cancellationToken: ct)
                  ?? throw new InvalidOperationException("Réponse météo vide");

        var main = dto.weather?.FirstOrDefault()?.main?.ToUpperInvariant();
        var weather = main switch
        {
            "RAIN" => Weather.RAIN,
            "THUNDERSTORM" => Weather.THUNDERSTORM,
            "SNOW" => Weather.SNOW,
            "CLOUDS" => Weather.CLOUDS,
            _ => Weather.CLEAR
        };

        await _cache.SetStringAsync(
            cacheKey,
            weather.ToString(),
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            },
            ct
        );

        return weather;
    }

    private sealed class OpenWeatherResp { public List<WeatherDesc>? weather { get; set; } }
    private sealed class WeatherDesc { public string? main { get; set; } public string? description { get; set; } }
}
