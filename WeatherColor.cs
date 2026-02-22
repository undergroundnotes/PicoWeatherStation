using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace DataVisualizer;

public static class WeatherColor
{
    // Fully contingent on StationData.cs
    public static float TEMP_MIN { get; private set; } = 0;
    public static float TEMP_MAX { get; private set; } = 19;

    public static float PRES_MIN { get; private set; } = 974;
    public static float PRES_MAX { get; private set; } = 976;

    public static float HUM_MIN { get; private set; } = 17;
    public static float HUM_MAX { get; private set; } = 44;

    public static float WIND_MIN { get; private set; } = 2000;
    public static float WIND_MAX { get; private set; } = 70000;

    private static float Normalize(float value, float min, float max)
    {
        return Math.Clamp((value - min) / (max - min), 0f, 1f);
    }

    public static Color ToColor(StationData weather)
    {
        float c = 1f - Normalize(weather.temperature, TEMP_MIN, TEMP_MAX);
        float m = Normalize(weather.pressure, PRES_MIN, PRES_MAX);
        float y = Normalize(weather.humidity, HUM_MIN, HUM_MAX);
        float k = Normalize(weather.windSpeed, WIND_MIN, WIND_MAX) / 1.25f;

        // https://www.rapidtables.com/convert/color/cmyk-to-rgb.html
        float r = (1f - c) * (1f - k);
        float g = (1f - m) * (1f - k);
        float b = (1f - y) * (1f - k);

        return new Color(r, g, b, 1f);
    }

    public static void SetRanges(List<WeatherStation> stations, bool ignoreHugeWind = true)
    {
        float tempMin = float.PositiveInfinity, tempMax = float.NegativeInfinity;
        float presMin = float.PositiveInfinity, presMax = float.NegativeInfinity;
        float humMin = float.PositiveInfinity, humMax = float.NegativeInfinity;
        float windMin = float.PositiveInfinity, windMax = float.NegativeInfinity;

        foreach (var s in stations)
        {
            foreach (WeatherData weather in s.WeatherData)
            {
                if (weather.temperature < tempMin) tempMin = weather.temperature;
                if (weather.temperature > tempMax) tempMax = weather.temperature;

                if (weather.pressure < presMin) presMin = weather.pressure;
                if (weather.pressure > presMax) presMax = weather.pressure;

                if (weather.humidity < humMin) humMin = weather.humidity;
                if (weather.humidity > humMax) humMax = weather.humidity;
            }

            foreach (WindData wd in s.WindData)
            {
                float v = wd.windspeed;

                if (ignoreHugeWind && v >= 60000f) continue;

                if (v < windMin) windMin = v;
                if (v > windMax) windMax = v;
            }
        }

        TEMP_MIN = tempMin; TEMP_MAX = tempMax;
        PRES_MIN = presMin; PRES_MAX = presMax;
        HUM_MIN = humMin; HUM_MAX = humMax;
        WIND_MIN = windMin; WIND_MAX = windMax;

        System.Console.WriteLine("==Min and Max Color Values==");
        System.Console.WriteLine($"Temperature: {TEMP_MIN}-{TEMP_MAX}\nPressure: {PRES_MIN}-{PRES_MAX}\nHumidity: {HUM_MIN}-{HUM_MAX}\nWindSpeed: {WIND_MIN}-{WIND_MAX}");
    }


}
