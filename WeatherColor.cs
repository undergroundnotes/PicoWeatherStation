using System;
using Microsoft.Xna.Framework;

namespace DataVisualizer;

public static class WeatherColor
{
    // Fully contingent on WeatherData.cs
    private static int TEMP_MIX = 15;
    private static int TEMP_MAX = 24;

    private static int PRES_MIX = 80;
    private static int PRES_MAX = 120;

    private static int HUM_MIX = 40;
    private static int HUM_MAX = 100;

    private static float Normalize(float value, float min, float max)
    {
        if (max <= min) return 0f;
        return Math.Clamp((value - min) / (max - min), 0f, 1f);
    }

    public static Color ToColor(WeatherData weather)
    {
        float c = Normalize(weather.temperature, TEMP_MIX, TEMP_MAX);
        float m = Normalize(weather.pressure, PRES_MIX, PRES_MAX);
        float y = Normalize(weather.humidity, HUM_MIX, HUM_MAX);
        float k = 0.5f;// TODO: WIND SPEED

        // https://www.rapidtables.com/convert/color/cmyk-to-rgb.html
        float r = (1f - c) * (1f - k);
        float g = (1f - m) * (1f - k);
        float b = (1f - y) * (1f - k);

        return new Color(r, g, b, 1f);
    }

}
