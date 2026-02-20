using System;
using Microsoft.Xna.Framework;

namespace DataVisualizer;

public static class WeatherColor
{
    // Fully contingent on WeatherData.cs
    private static int TEMP_MIN = 15;
    private static int TEMP_MAX = 24;

    private static int PRES_MIN = 80;
    private static int PRES_MAX = 120;

    private static int HUM_MIN = 40;
    private static int HUM_MAX = 100;

    private static int WIND_MIN = 40;
    private static int WIND_MAX = 100;

    private static float Normalize(float value, float min, float max)
    {
        if (max <= min) return 0f;
        return Math.Clamp((value - min) / (max - min), 0f, 1f);
    }

    public static Color ToColor(WeatherData weather)
    {
        float c = Normalize(weather.temperature, TEMP_MIN, TEMP_MAX);
        float m = Normalize(weather.pressure, PRES_MIN, PRES_MAX);
        float y = Normalize(weather.humidity, HUM_MIN, HUM_MAX);
        float k = 0.5f;// TODO: WIND SPEED

        // https://www.rapidtables.com/convert/color/cmyk-to-rgb.html
        float r = (1f - c) * (1f - k);
        float g = (1f - m) * (1f - k);
        float b = (1f - y) * (1f - k);

        return new Color(r, g, b, 1f);
    }

    /*public static WeatherData ColorToWeather(Color color)
    {
        // Reverse the color; find normal; scale normal to range

        float r = color.R;
        float g = color.G;
        float b = color.B;

        // This is impossible...
        // If I want to do this I cannot read color and get data
        // I will need to make each pixel a thing, which I dont want to do
    }*/

}
