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
    private readonly float _isobarPressureStep;


    public WeatherMap(GraphicsDevice gd, Rectangle destRect, List<WeatherStation> stations, float isoBarStep, int stationRadius = 10)
    {
        DestRect = destRect;
        this.stations = stations;
        _stationRadius = stationRadius;

        _stationTex = CreateCircleTexture(gd, _stationRadius, Color.White, 4, Color.Black);

        _fieldTex = new Texture2D(gd, DestRect.Width, DestRect.Height);
        _fieldPixels = new Color[DestRect.Width * DestRect.Height];

        _isobarPressureStep = isoBarStep;
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

        return LerpStationData(simulationTime, stationDataA, stationDataB, linePos);
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

    public void RegenerateFieldTexture(float simulationTime, WeatherStation stationA, WeatherStation stationB)
    {
        Vector2 A = NormalizedToScreen(stationA.NormalizedPosition);
        Vector2 B = NormalizedToScreen(stationB.NormalizedPosition);
        float distance = Vector2.Distance(A, B);

        StationData dataA = stationA.GetStationDataAtTime(simulationTime);
        StationData dataB = stationB.GetStationDataAtTime(simulationTime);

        float localPressureRange = MathF.Abs(dataB.Pressure - dataA.Pressure);

        int isobarCount = localPressureRange >= _isobarPressureStep ? (int)(localPressureRange / _isobarPressureStep) : 0;

        float isobarToleranceT = 1f / distance;

        for (int y = 0; y < DestRect.Height; y++)
        {
            for (int x = 0; x < DestRect.Width; x++)
            {
                Vector2 p = new Vector2(DestRect.X + x, DestRect.Y + y);

                float projection = ProjectOnLine(A, B, p);

                StationData blended = LerpStationData(simulationTime, dataA, dataB, projection);
                Color pixelColor = WeatherColor.ToColor(blended);

                if (isobarCount > 0)
                {
                    // hack to find which "index" of isobar we are on
                    // where projection is some point on the Gradient
                    int isobarIndex = (int)MathF.Round(projection * (localPressureRange / _isobarPressureStep));

                    if (isobarIndex >= 1 && isobarIndex <= isobarCount)
                    {
                        float localIso = (isobarIndex * _isobarPressureStep) / localPressureRange;
                        if (MathF.Abs(projection - localIso) <= isobarToleranceT)
                            pixelColor = Color.Black;
                    }
                }

                _fieldPixels[y * DestRect.Width + x] = pixelColor;
            }
        }

        _fieldTex.SetData(_fieldPixels);
    }

    private StationData LerpStationData(float reading, StationData a, StationData b, float t)
    {
        return new StationData(
            Time: reading,// This makes 0 sense for what it is.
            Temperature: MathHelper.Lerp(a.Temperature, b.Temperature, t),
            Pressure: MathHelper.Lerp(a.Pressure, b.Pressure, t),
            Humidity: MathHelper.Lerp(a.Humidity, b.Humidity, t),
            WindSpeed: MathHelper.Lerp(a.WindSpeed, b.WindSpeed, t)
        );
        
    }

    private float ProjectOnLine(Vector2 a, Vector2 b, Vector2 p)
    {
        // I need to project arbitray point p, on the line created from a to b
        // projection formula!
        Vector2 ab = b - a;
        float denom = Vector2.Dot(ab, ab);
        if (denom <= 0.0001f) return 0f; // blessed
        float pos = Vector2.Dot(p - a, ab) / denom;
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