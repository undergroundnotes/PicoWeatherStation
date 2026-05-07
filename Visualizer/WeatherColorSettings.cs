namespace DataVisualizer;

public record struct WeatherColorSettings(
    double TemperatureMin,
    double TemperatureMax,

    double PressureMin,
    double PressureMax,

    double HumidityMin,
    double HumidityMax,

    double WindMin,
    double WindMax
)
{}
