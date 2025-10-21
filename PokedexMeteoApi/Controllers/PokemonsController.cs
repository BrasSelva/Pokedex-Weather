using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PokedexMeteoApi.Domain.Entities;
using PokedexMeteoApi.Infrastructure;


namespace PokedexWeatherApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PokemonsController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Pokemon>>> GetAll() => await db.Pokemons.AsNoTracking().ToListAsync();

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Pokemon>> Get(int id) => await db.Pokemons.FindAsync(id) is { } p ? Ok(p) : NotFound();

    public record CreatePokemonDto(string Name, PokemonType Type);

    [HttpPost]
    public async Task<ActionResult<Pokemon>> Create(CreatePokemonDto dto)
    {
        var p = new Pokemon { Name = dto.Name, Type = dto.Type };
        db.Pokemons.Add(p);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = p.Id }, p);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, CreatePokemonDto dto)
    {
        var p = await db.Pokemons.FindAsync(id);
        if (p is null) return NotFound();
        p.Name = dto.Name; p.Type = dto.Type;
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var p = await db.Pokemons.FindAsync(id);
        if (p is null) return NotFound();
        db.Pokemons.Remove(p);
        await db.SaveChangesAsync();
        return NoContent();
    }
}
