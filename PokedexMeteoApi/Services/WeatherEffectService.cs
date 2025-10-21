using PokedexMeteoApi.Domain.Entities;

namespace PokedexMeteoApi.Services;

public class WeatherEffectService : IWeatherEffectService
{
    public WeatherEffect Evaluate(PokemonType type, Weather weather)
    {
        return weather switch
        {
            Weather.RAIN  => type switch
            {
                PokemonType.Fire => WeatherEffect.WEAK,
                PokemonType.Water or PokemonType.Electric => WeatherEffect.RESISTANT,
                _ => WeatherEffect.NEUTRAL
            },
            Weather.SNOW => type switch
            {
                PokemonType.Ice => WeatherEffect.RESISTANT,
                _ => WeatherEffect.NEUTRAL
            },
            Weather.THUNDERSTORM => type switch
            {
                PokemonType.Water => WeatherEffect.WEAK,
                PokemonType.Electric => WeatherEffect.RESISTANT,
                _ => WeatherEffect.NEUTRAL
            },
            Weather.CLEAR => WeatherEffect.NEUTRAL,
            Weather.CLOUDS => WeatherEffect.NEUTRAL,
            _ => WeatherEffect.NEUTRAL
        };
    }
}
