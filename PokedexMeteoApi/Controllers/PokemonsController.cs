using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PokedexMeteoApi.Domain.Entities;
using PokedexMeteoApi.Infrastructure;

namespace PokedexWeatherApi.Controllers;


[ApiController]
// Définit la route de base 
// Exemple : https://localhost:7279/api/pokemons
[Route("api/[controller]")]
public class PokemonsController(AppDbContext db) : ControllerBase
{
    // GET /api/pokemons
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Pokemon>>> GetAll()
        => await db.Pokemons.AsNoTracking().ToListAsync();

    // GET /api/pokemons/{id}
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Pokemon>> Get(int id)
        => await db.Pokemons.FindAsync(id) is { } pokemon ? Ok(pokemon) : NotFound();

    // Sert à recevoir les données envoyées par le client lors d’un POST ou PUT
    public record CreatePokemonDto(string Name, PokemonType Type);

    // POST /api/pokemons
    [HttpPost]
    public async Task<ActionResult<Pokemon>> Create(CreatePokemonDto dto)
    {
        // Crée un nouvel objet à partir des données reçues
        var pokemon = new Pokemon { Name = dto.Name, Type = dto.Type };

        db.Pokemons.Add(pokemon);

        await db.SaveChangesAsync();

        // Retourne un code 201
        return CreatedAtAction(nameof(Get), new { id = pokemon.Id }, pokemon);
    }

    // PUT /api/pokemons/{id}
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, CreatePokemonDto dto)
    {
        var pokemon = await db.Pokemons.FindAsync(id);
        if (pokemon is null) return NotFound(); // 404 si pas trouvé

        // Mise à jour des champs
        pokemon.Name = dto.Name;
        pokemon.Type = dto.Type;

        await db.SaveChangesAsync();

        return NoContent();
    }

    // DELETE /api/pokemons/{id}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var pokemon = await db.Pokemons.FindAsync(id);
        if (pokemon is null) return NotFound();

        db.Pokemons.Remove(pokemon);

        await db.SaveChangesAsync();

        return NoContent();
    }
}
