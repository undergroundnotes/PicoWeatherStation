namespace DataVisualizer;

public record struct WeatherData(float time, float temperature, float pressure, float humidity)
{
    public override string ToString()
    {
        return $"Time:{time}\nTemperature:{temperature}\nPressure:{pressure}\nHumidity:{humidity}";
    }
}
