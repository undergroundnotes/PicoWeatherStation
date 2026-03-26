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
        //return FromCYMK(weather);
        return FromHSL(weather);
        //return FromRGB(weather);
    }

    private static Color FromRGB(StationData weather)
    {
        float r = Normalize(weather.Temperature, TEMP_MIN, TEMP_MAX);
        float g = Normalize(weather.Humidity, HUM_MIN, HUM_MAX);
        float b = Normalize(weather.WindSpeed, WIND_MIN, WIND_MAX);

        return new Color(r, g, b, 1f);
    }

    private static Color FromHSL(StationData weather)
    {
        float temp = Normalize(weather.Temperature, TEMP_MIN, TEMP_MAX);
        float hum = Normalize(weather.Humidity, HUM_MIN, HUM_MAX);
        float wind = Normalize(weather.WindSpeed, WIND_MIN, WIND_MAX);
        // https://www.rapidtables.com/convert/color/hsl-to-rgb.html

        // Normalization makes the values from 0 to
        float h = MathHelper.Lerp(230f, 0f, temp); // This puts it in 0..360, however starting from 220, which is blue 360 red
        float s = MathHelper.Lerp(0.25f, 1f, hum);
        float l = MathHelper.Lerp(0.5f, 0.9f, wind);


        float c = (1f - MathF.Abs(2f * l - 1f)) * s;
        float x = c * (1f - MathF.Abs(((h / 60) % 2f) - 1f));
        float m = l - c / 2f;

        float r1, g1, b1;
        if (h < 60f) { r1 = c; g1 = x; b1 = 0f; }
        else if (h < 120) { r1 = x; g1 = c; b1 = 0f; }
        else if (h < 180) { r1 = 0f; g1 = c; b1 = x; }
        else if (h < 240) { r1 = 0f; g1 = x; b1 = c; }
        else if (h < 300) { r1 = x; g1 = 0f; b1 = c; }
        else { r1 = c; g1 = 0f; b1 = x; }

        float r = (r1 + m);
        float g = (g1 + m);
        float b = (b1 + m);

        return new Color(r, g, b);
    }

    private static Color FromCYMK(StationData weather)
    {
        float c = MathHelper.Lerp(0.2f, 1, 1f - Normalize(weather.Temperature, TEMP_MIN, TEMP_MAX));
        float m = MathHelper.Lerp(0.2f, 1, Normalize(weather.Pressure, PRES_MIN, PRES_MAX));
        float y = MathHelper.Lerp(0.2f, 1, Normalize(weather.Humidity, HUM_MIN, HUM_MAX));
        float k = MathHelper.Lerp(0.2f, 0.7f, Normalize(weather.WindSpeed, WIND_MIN, WIND_MAX) / 1.25f);

        // https://www.rapidtables.com/convert/color/cmyk-to-rgb.html
        float r = (1f - c) * (1f - k);
        float g = (1f - m) * (1f - k);
        float b = (1f - y) * (1f - k);

        return new Color(r, g, b, 1f);
    }

    // No longer needed as these are directly read, so no min/max can be generated
    /*public static void SetRanges(List<WeatherStation> stations, bool ignoreHugeWind = true)
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
                float v = wd.WindSpeed;

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
    }*/


}
