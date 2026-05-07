using System;

namespace DataVisualizer;

// todo: probably use date time instead of float
public record struct StationData(DateTime Time, float Temperature, float Pressure, float Humidity, float WindSpeed)
{
    public override string ToString()
    {
        return $"Time: {Time:yyyy-MM-dd HH:mm:ss}\n"
        + "Temperature(C): " + Temperature + "\n"
        + "Humidity(%): " + Humidity + "\n"
        + "Wind(km/h): " + WindSpeed + "\n"
        + "Pressure(hPa): " + Pressure;
    }
}
