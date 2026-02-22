using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace DataVisualizer;

public class WeatherMap
{
    public Rectangle DestRect { get; set; }

    private readonly List<WeatherStation> stations;
    private readonly Texture2D _stationTex;
    private readonly int _stationRadius;

    private Texture2D _fieldTex;
    private Color[] _fieldPixels;

    private readonly float _isoStep = 0.05f;
    private List<float> isoBarLocations = new List<float>();

    public WeatherMap(GraphicsDevice gd, Rectangle destRect, List<WeatherStation> stations, int stationRadius = 10, float isoBarStep = 0.05f)
    {
        DestRect = destRect;
        this.stations = stations;
        _stationRadius = stationRadius;

        _stationTex = CreateCircleTexture(gd, _stationRadius, Color.White, 4, Color.Black);

        _fieldTex = new Texture2D(gd, DestRect.Width, DestRect.Height);
        _fieldPixels = new Color[DestRect.Width * DestRect.Height];

        _isoStep = isoBarStep;
    }

    public Vector2 NormalizedToScreen(Vector2 uv)
    {
        return new Vector2(DestRect.X + (uv.X * DestRect.Width), DestRect.Y + (uv.Y * DestRect.Height));
    }

    public WeatherStation GetStationMouseOverlap(MouseState mouse)
    {
        Vector2 mousePosition = new Vector2(mouse.X, mouse.Y);
        if (!DestRect.Contains(mousePosition.X, mousePosition.Y)) return null;

        foreach (var s in stations)
        {
            Vector2 stationPosition = NormalizedToScreen(s.NormalizedPosition);
            if (Vector2.Distance(stationPosition, mousePosition) <= _stationRadius)
                return s;
        }
        return null;
    }

    public StationData? GetInterpolatedDataAtMouse(MouseState mouse, float simulationTime, WeatherStation a, WeatherStation b)
    {
        if (!DestRect.Contains(mouse.X, mouse.Y))
            return null;

        Vector2 p = new Vector2(mouse.X, mouse.Y);

        Vector2 A = NormalizedToScreen(a.NormalizedPosition);
        Vector2 B = NormalizedToScreen(b.NormalizedPosition);

        float linePos = ProjectOnLine(A, B, p);

        StationData stationDataA = a.GetStationDataAtTime(simulationTime);
        StationData stationDataB = b.GetStationDataAtTime(simulationTime);

        return new StationData(
            weatherTimeReading: simulationTime,
            WindTimeReading: simulationTime,
            temperature: MathHelper.Lerp(stationDataA.temperature, stationDataB.temperature, linePos),
            pressure: MathHelper.Lerp(stationDataA.pressure, stationDataB.pressure, linePos),
            humidity: MathHelper.Lerp(stationDataA.humidity, stationDataB.humidity, linePos),
            windSpeed: MathHelper.Lerp(stationDataA.windSpeed, stationDataB.windSpeed, linePos)
        );
    }

    public void Draw(SpriteBatch sb, float simulationTime)
    {
        sb.Draw(_fieldTex, DestRect, Color.White);

        foreach (WeatherStation s in stations)
        {
            Vector2 staionRenderPosition = NormalizedToScreen(s.NormalizedPosition);

            Color stationColor = WeatherColor.ToColor(s.GetStationDataAtTime(simulationTime));

            sb.Draw(_stationTex, staionRenderPosition, null, stationColor, 0f, new Vector2(_stationRadius), 1f, SpriteEffects.None, 0f);
        }
    }

    public void RegenerateFieldTexture(float simulationTime, WeatherStation a, WeatherStation b)
    {
        Vector2 A = NormalizedToScreen(a.NormalizedPosition);
        Vector2 B = NormalizedToScreen(b.NormalizedPosition);

        float distance = Vector2.Distance(A, B);
        if (distance < 0.001f) return;

        StationData stationDataA = a.GetStationDataAtTime(simulationTime);
        StationData stationDataB = b.GetStationDataAtTime(simulationTime);

        float startPressure = stationDataA.pressure;
        float endPressure = stationDataB.pressure;
        float globalDiff = WeatherColor.PRES_MAX - WeatherColor.PRES_MIN;
        float localDiff = endPressure - startPressure;

        isoBarLocations.Clear();
        for (float i = _isoStep; i < 1f; i += _isoStep)
        {
            // Ok. I want to calculate where the isobars will be representing a 5% change (determined by min max)

            // The local gradient is 0.25
            // global min is 1, gmax is 11. thus diff is 10
            // I want to have an iso bar is 2.5, 5, 7.5 (global step is 2.5)
            // offset by the start, so global isobars are at 3.5, 6, 8.5
            float globalStep = i * globalDiff;
            float globalIsobar = globalStep + WeatherColor.PRES_MIN;

            // local min is 2, lmax is 8. dif is 6
            // So on my local scale: STARTING at 2. isobars are at 4.5, 7
            // from before: iso = start + i * diff...
            // I lost my goat explination when this file got corrupted, but whatever.
            // I dont want to necessarily convert scale, I want to USE the global scale, but for the range of local
            // isoBarAtStep * localDiff + globalIsobar = pressure(step)
            float isoBarAtStep = (globalIsobar - startPressure) / localDiff;

            if (isoBarAtStep >= 0f && isoBarAtStep <= 1f)
                isoBarLocations.Add(isoBarAtStep);
        }

        float isoThickness = 1f;
        float lineTorlerance = isoThickness / distance;

        for (int y = 0; y < DestRect.Height; y++)
        {
            for (int x = 0; x < DestRect.Width; x++)
            {
                Vector2 p = new Vector2(DestRect.X + x, DestRect.Y + y);

                float linePosition = ProjectOnLine(A, B, p);

                StationData blended = new StationData(
                    weatherTimeReading: simulationTime, // TODO: change
                    WindTimeReading: simulationTime, // TODO: change
                    temperature: MathHelper.Lerp(stationDataA.temperature, stationDataB.temperature, linePosition),
                    pressure: MathHelper.Lerp(stationDataA.pressure, stationDataB.pressure, linePosition),
                    humidity: MathHelper.Lerp(stationDataA.humidity, stationDataB.humidity, linePosition),
                    windSpeed: MathHelper.Lerp(stationDataA.windSpeed, stationDataB.windSpeed, linePosition)
                );

                Color pixelColor = WeatherColor.ToColor(blended);

                // post color step
                for (int i = 0; i < isoBarLocations.Count; i++)
                {
                    if (MathF.Abs(linePosition - isoBarLocations[i]) <= lineTorlerance)
                    {
                        pixelColor = Color.Black;
                        break;
                    }
                }

                _fieldPixels[y * DestRect.Width + x] = pixelColor;
            }
        }

        _fieldTex.SetData(_fieldPixels);
    }

    private float ProjectOnLine(Vector2 a, Vector2 b, Vector2 p)
    {
        // I need to project arbitray point p, on the line created from a to b
        // projection formula!
        Vector2 ab = b - a;
        float pos = Vector2.Dot(p - a, ab) / Vector2.Dot(ab, ab);
        return MathHelper.Clamp(pos, 0f, 1f);
    }

    private Texture2D CreateCircleTexture(GraphicsDevice graphicsDevice, int radius, Color fillColor, int stroke, Color strokeColor)
    {
        int diameter = radius * 2;
        Texture2D texture = new Texture2D(graphicsDevice, diameter, diameter);

        Color[] data = new Color[diameter * diameter];

        Vector2 center = new Vector2(radius);

        for (int y = 0; y < diameter; y++)
        {
            for (int x = 0; x < diameter; x++)
            {
                Vector2 pos = new Vector2(x, y);
                float distance = Vector2.Distance(pos, center);

                int index = y * diameter + x;

                if (distance <= radius)
                {
                    if (distance >= radius - stroke)
                        data[index] = strokeColor;
                    else
                        data[index] = fillColor;

                }
                else
                {
                    data[index] = Color.Transparent;
                }
            }
        }

        texture.SetData(data);
        return texture;
    }
}