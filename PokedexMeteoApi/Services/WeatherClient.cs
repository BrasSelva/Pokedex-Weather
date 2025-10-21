using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using System.Text;

namespace PokedexMeteoApi.Services;

public sealed class WeatherClient : IWeatherClient
{

    private readonly HttpClient _http;              
    private readonly IDistributedCache _cache;      
    private readonly string _apiKey;                
    private readonly string _baseUrl;               

    public WeatherClient(HttpClient http, IConfiguration cfg, IDistributedCache cache)
    {
        _http = http;
        _cache = cache;

        // Récupération des valeurs dans la configuration (UserSecrets ou appsettings)
        _apiKey = cfg["Weather:ApiKey"] ?? "";
        _baseUrl = (cfg["Weather:BaseUrl"] ?? "https://api.openweathermap.org/data/2.5").TrimEnd('/');
    }

    // Récupère la météo d’une ville, avec gestion du cache Redis
    public async Task<Weather> GetWeatherByCityAsync(string city, CancellationToken cancellationToken = default)
    {
        // Vérifie que la clé API est présente
        if (string.IsNullOrWhiteSpace(_apiKey))
            throw new InvalidOperationException("Weather API key is missing.");

        // Définit une clé de cache basée sur le nom de la ville (ex: weather:paris)
        var cacheKey = $"weather:{city.ToLower()}";

        // Vérifie si la météo est déjà en cache
        var cached = await _cache.GetStringAsync(cacheKey, cancellationToken);

        // Si trouvée → on la renvoie directement sans appeler l’API (gain de temps et quota)
        if (!string.IsNullOrEmpty(cached))
        {
            Console.WriteLine($"[CACHE] Hit pour {city}");
            return Enum.Parse<Weather>(cached); 
        }

        // Sinon on va chercher la météo via l’API OpenWeather
        Console.WriteLine($"[CACHE] Miss pour {city}");

        // Nettoie et encode le nom de la ville
        var q = Uri.EscapeDataString(city.Trim());

        // 5onstruit l’URL complète pour l’appel API
        // Exemple : https://api.openweathermap.org/data/2.5/weather?q=Paris&appid=xxxx&units=metric&lang=fr
        var url = $"{_baseUrl}/weather?q={q}&appid={_apiKey}&units=metric&lang=fr";

        using var resp = await _http.GetAsync(url, cancellationToken);
        resp.EnsureSuccessStatusCode(); 

        // Désérialise le JSON renvoyé par OpenWeather
        var dto = await resp.Content.ReadFromJsonAsync<OpenWeatherResp>(cancellationToken: cancellationToken) ?? throw new InvalidOperationException("Réponse météo vide");


        var main = dto.weather?.FirstOrDefault()?.main?.ToUpperInvariant();

        var weather = main switch
        {
            "RAIN" => Weather.RAIN,
            "THUNDERSTORM" => Weather.THUNDERSTORM,
            "SNOW" => Weather.SNOW,
            "CLOUDS" => Weather.CLOUDS,
            _ => Weather.CLEAR // par défaut
        };

        // 9Stocke la valeur dans Redis pour 5 minutes
        await _cache.SetStringAsync(
            cacheKey,              
            weather.ToString(),    
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            },
            cancellationToken
        );

        return weather;
    }

    // Classes internes pour mapper la réponse JSON d’OpenWeather ---

    private sealed class OpenWeatherResp
    {
        public List<WeatherDesc>? weather { get; set; }
    }

    private sealed class WeatherDesc
    {
        public string? main { get; set; }          
        public string? description { get; set; }   
    }
}
