using System;
using System.Runtime.CompilerServices;
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

    private static float _tempInvRange;
    private static float _presInvRange;
    private static float _humInvRange;
    private static float _windInvRange;

    public static Color ToColor(StationData weather)
    {
        return FromHSL(weather);
    }

    public static float WindScale(StationData station)
    {
        return 0.5f + (Normalize(station.WindSpeed, WIND_MIN, _windInvRange) * 2f);
    }

    private static Color FromHSL(StationData weather)
    {
        float temp = Normalize(weather.Temperature, TEMP_MIN, _tempInvRange);
        float hum = Normalize(weather.Humidity, HUM_MIN, _humInvRange);
        //float wind = 0f;//Normalize(weather.WindSpeed, WIND_MIN, WIND_MAX);
        // https://www.rapidtables.com/convert/color/hsl-to-rgb.html

        // Normalization makes the values from 0 to
        float h = MathHelper.Lerp(230f, 0f, temp); // This puts it in 0..360, however starting from 220, which is blue 360 red
        float s = MathHelper.Lerp(0.25f, 1f, hum);
        float l = 0.5f;//MathHelper.Lerp(0.5f, 0.9f, wind);


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

    public static void SetValues(WeatherColorSettings settings)
    {
        TEMP_MIN = (float)settings.TemperatureMin;
        TEMP_MAX = (float)settings.TemperatureMax;

        PRES_MIN = (float)settings.PressureMin;
        PRES_MAX = (float)settings.PressureMax;

        HUM_MIN = (float)settings.HumidityMin;
        HUM_MAX = (float)settings.HumidityMax;

        WIND_MIN = (float)settings.WindMin;
        WIND_MAX = (float)settings.WindMax;

        _tempInvRange = CalculateInvRange(TEMP_MIN, TEMP_MAX);
        _presInvRange = CalculateInvRange(PRES_MIN, PRES_MAX);
        _humInvRange = CalculateInvRange(HUM_MIN, HUM_MAX);
        _windInvRange = CalculateInvRange(WIND_MIN, WIND_MAX);
    }


    // ----

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float Normalize(float value, float min, float invRange)
    {
        float normalized = (value - min) * invRange;

        if (normalized < 0f)
            return 0f;

        if (normalized > 1f)
            return 1f;

        return normalized;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float CalculateInvRange(float min, float max)
    {
        float range = max - min;
        return range > 0f ? 1f / range : 0f;
    }
}
