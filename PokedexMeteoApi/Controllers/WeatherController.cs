using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PokedexMeteoApi.Infrastructure;
using PokedexMeteoApi.Services;

namespace PokedexWeatherApi.Controllers;

[ApiController]
// Définit la route racine : /api/weather
[Route("api/weather")]
public class WeatherController(AppDbContext db, IWeatherClient meteo, IWeatherEffectService rules) : ControllerBase
{
    // GET /api/weather/effect
    [HttpGet("effect")]
    public async Task<IActionResult> GetEffect(
        [FromQuery] int pokemonId,          
        [FromQuery] string city,            
        CancellationToken cancellantionTokent)  
    {
        if (string.IsNullOrWhiteSpace(city))
            return BadRequest("Paramètre 'city' requis."); // → 400 Bad Request

        var pokemon = await db.Pokemons.AsNoTracking().FirstOrDefaultAsync(x => x.Id == pokemonId, cancellantionTokent);

        if (pokemon is null)
            return NotFound("Pokemon not found"); 

        try
        {
            // Appel de l’API météo (
            var weather = await meteo.GetWeatherByCityAsync(city, cancellantionTokent);

            
            var eff = rules.Evaluate(pokemon.Type, weather);

            return Ok(new
            {
                pokemonName = pokemon.Name,          
                type = pokemon.Type.ToString(),      
                city,                                
                weather = weather.ToString(),        
                effect = eff                        
            });
        }


        // Si le service météo ne trouve pas la ville (clé manquante dans les résultats)
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message); // → 404 Not Found
        }

        // Si l’API météo retourne une erreur liée à la clé (API key invalide)
        catch (InvalidOperationException ex) when (ex.Message.Contains("API key", StringComparison.OrdinalIgnoreCase))
        {
            return Problem(
                statusCode: 502,                     
                title: "Weather provider error",     
                detail: ex.Message                  
            );
        }

        // Si une erreur réseau survient (ex: API indisponible)
        catch (HttpRequestException ex)
        {
            return Problem(
                statusCode: 502,                     
                title: "Weather provider error",
                detail: ex.Message
            );
        }
    }
}
