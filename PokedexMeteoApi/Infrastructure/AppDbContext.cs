using Microsoft.EntityFrameworkCore;
using PokedexMeteoApi.Domain.Entities;

namespace PokedexMeteoApi.Infrastructure
{
    // Classe principale de configuration de la base de données avec Entity Framework Core

    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Pokemon> Pokemons => Set<Pokemon>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<Pokemon>()
                        .Property(p => p.Type)
                        .HasConversion<string>();

            // Ces données seront insérées automatiquement à la migration ou création de la BDD.
            modelBuilder.Entity<Pokemon>().HasData(
                new Pokemon { Id = 1, Name = "Dracaufeu", Type = PokemonType.Fire },
                new Pokemon { Id = 2, Name = "Pikachu", Type = PokemonType.Electric },
                new Pokemon { Id = 3, Name = "Psykokwak", Type = PokemonType.Psychic },
                new Pokemon { Id = 4, Name = "Magicarpe", Type = PokemonType.Water }
            );
        }
    }
}
