using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PokedexMeteoApi.Infrastructure;
using PokedexMeteoApi.Services;

namespace PokedexWeatherApi.Controllers;

[ApiController]
[Route("api/weather")]
public class WeatherController(AppDbContext db, IWeatherClient meteo, IWeatherEffectService rules) : ControllerBase
{
    [HttpGet("effect")]
    public async Task<IActionResult> GetEffect([FromQuery] int pokemonId, [FromQuery] string city, CancellationToken cancellantionTokent)
    {
        if (string.IsNullOrWhiteSpace(city))
            return BadRequest("Paramètre 'city' requis.");

        var pokemon = await db.Pokemons.AsNoTracking().FirstOrDefaultAsync(x => x.Id == pokemonId, cancellantionTokent);
        if (pokemon is null) return NotFound("Pokemon not found");

        try
        {
            var weather = await meteo.GetWeatherByCityAsync(city, cancellantionTokent);
            var eff = rules.Evaluate(pokemon.Type, weather);
            return Ok(new { pokemonName = pokemon.Name, type = pokemon.Type.ToString(), city, weather = weather.ToString(), effect = eff });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message); 
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("API key", StringComparison.OrdinalIgnoreCase))
        {
            return Problem(statusCode: 502, title: "Weather provider error", detail: ex.Message);
        }
        catch (HttpRequestException ex)
        {
            return Problem(statusCode: 502, title: "Weather provider error", detail: ex.Message);
        }
    }
}