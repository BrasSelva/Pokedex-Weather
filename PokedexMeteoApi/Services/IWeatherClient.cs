namespace PokedexMeteoApi.Services;

public enum Weather
{
    CLEAR,
    CLOUDS,
    RAIN,
    THUNDERSTORM,
    SNOW,
    EXTREME,
    WIND,
}


public interface IWeatherClient
{
    Task<Weather> GetWeatherByCityAsync(string city, CancellationToken ct = default);
}
