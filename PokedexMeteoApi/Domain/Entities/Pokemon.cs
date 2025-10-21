namespace PokedexMeteoApi.Domain.Entities
{
    public enum PokemonType
    {
        Normal,
        Fire,
        Water,
        Electric,
        Ice,
        Poison,
        Psychic,
        Dragon
    }

    public class Pokemon
    {
        public int Id { get; set; }
        public string? Name{ get; set; }

        public PokemonType Type { get; set; }
    }
}
