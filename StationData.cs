namespace DataVisualizer;

// Used as a temperary intermediate to combine both wind and weather readings, as they are seperate
public record struct StationData(float weatherTimeReading, float WindTimeReading, float temperature, float pressure, float humidity, float windSpeed)
{
    public override string ToString()
    {
        return $"Weather({weatherTimeReading}):\n" +
        $"\tTemperature: {temperature}\n\tPressure: {pressure}\n\tHumidity: {humidity}\n" +
        $"Wind({WindTimeReading}):\n" +
        $"\tWindspeed {windSpeed}";
    }
}
