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

    public WeatherMap(GraphicsDevice gd, Rectangle destRect, List<WeatherStation> stations, int stationRadius = 10)
    {
        DestRect = destRect;
        this.stations = stations;
        _stationRadius = stationRadius;

        _stationTex = CreateCircleTexture(gd, _stationRadius, Color.White, 4, Color.Black);

        _fieldTex = new Texture2D(gd, DestRect.Width, DestRect.Height);
        _fieldPixels = new Color[DestRect.Width * DestRect.Height];
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

    public WeatherData? GetWeatherDataAtMouse(MouseState mouse)
    {
        return null;
    }

    public void Draw(SpriteBatch sb, float simulationTime)
    {
        sb.Draw(_fieldTex, DestRect, Color.White);

        foreach (WeatherStation s in stations)
        {
            Vector2 staionRenderPosition = NormalizedToScreen(s.NormalizedPosition);

            Color stationColor = WeatherColor.ToColor(s.GetReadingAtTime(simulationTime));

            sb.Draw(_stationTex, staionRenderPosition, null, stationColor, 0f, new Vector2(_stationRadius), 1f, SpriteEffects.None, 0f);
        }
    }

    public void RegenerateFieldTexture(float simulationTime, WeatherStation a, WeatherStation b)
    {
        float SegmentT(Vector2 a, Vector2 b, Vector2 p)
        {
            Vector2 ab = b - a;
            float denom = Vector2.Dot(ab, ab);
            if (denom <= 1e-8f) return 0f;
            float t = Vector2.Dot(p - a, ab) / denom;
            return MathHelper.Clamp(t, 0f, 1f);
        }

        // n=2: linear along segment in SCREEN space
        Vector2 A = NormalizedToScreen(a.NormalizedPosition);
        Vector2 B = NormalizedToScreen(b.NormalizedPosition);

        int w = DestRect.Width;
        int h = DestRect.Height;

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                Vector2 p = new Vector2(DestRect.X + x, DestRect.Y + y);

                float t = SegmentT(A, B, p);

                WeatherData wa = a.GetReadingAtTime(simulationTime);
                WeatherData wb = b.GetReadingAtTime(simulationTime);

                WeatherData blended = new WeatherData(
                    simulationTime,
                    MathHelper.Lerp(wa.temperature, wb.temperature, t),
                    MathHelper.Lerp(wa.pressure, wb.pressure, t),
                    MathHelper.Lerp(wa.humidity, wb.humidity, t)
                );

                _fieldPixels[y * w + x] = WeatherColor.ToColor(blended);
            }
        }

        _fieldTex.SetData(_fieldPixels);
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