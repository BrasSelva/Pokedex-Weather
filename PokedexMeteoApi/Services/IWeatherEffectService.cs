using PokedexMeteoApi.Domain.Entities;

namespace PokedexMeteoApi.Services;

public enum WeatherEffect { WEAK, RESISTANT, NEUTRAL }

public interface IWeatherEffectService
{
    WeatherEffect Evaluate(PokemonType type, Weather weather);
}
