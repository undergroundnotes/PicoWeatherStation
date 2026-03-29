using DataVisualizerLive;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace DataVisualizer;

public class WeatherMap
{
    private Rectangle _destRect { get; set; }
    private readonly Texture2D _stationTex;
    private readonly Texture2D _fieldTex;
    private readonly Color[] _fieldPixels;
    private readonly float _isobarPressureStep;

    private readonly int _fieldWidth;
    private readonly int _fieldHeight;

    public WeatherMap(GraphicsDevice gd, Rectangle destRect, float isoBarStep, float scale = 1f, Texture2D stationTexture = null)
    {
        _destRect = destRect;
        _isobarPressureStep = isoBarStep;

        _stationTex = stationTexture ?? Utils.CreateCircleTexture(gd, 12, Color.White, 3, Color.Black);

        _fieldWidth = (int)(_destRect.Width * scale);
        _fieldHeight = (int)(_destRect.Height * scale);

        _fieldTex = new Texture2D(gd, _fieldWidth, _fieldHeight);
        _fieldPixels = new Color[_fieldWidth * _fieldHeight];
    }

    public WeatherStation GetStationMouseOverlap(MouseState mouse, List<WeatherStation> stations)
    {
        if (!_destRect.Contains(mouse.X, mouse.Y)) return null;

        Vector2 mousePosition = ScreenToField(new Vector2(mouse.X, mouse.Y));

        foreach (var s in stations)
        {
            if (!s.HasData) continue;

            if (Vector2.Distance(mousePosition, s.FieldPosition) <= _stationTex.Width / 2f/* * WeatherColor.WindScale(s.GetLatestData().Value)*/)
                return s;
        }

        return null;
    }

    public StationData? GetInterpolatedDataAtMouse(MouseState mouse, List<WeatherStation> stations)
    {
        if (!_destRect.Contains(mouse.X, mouse.Y) || stations.Count < 2)
            return null;

        Vector2 p = ScreenToField(new Vector2(mouse.X, mouse.Y));

        return InterpolateIDW(p, stations);
    }

    public void Draw(SpriteBatch spriteBatch, List<WeatherStation> stations, float alpha = 1f)
    {
        spriteBatch.Draw(_fieldTex, _destRect, Color.White * alpha);

        foreach (WeatherStation station in stations)
        {
            StationData? data = station.GetLatestData();
            if (!data.HasValue)
                continue;

            Vector2 staionRenderPosition = FieldToScreen(station.FieldPosition);

            Color color = WeatherColor.ToColor(data.Value);

            float stationScale = 1f;//WeatherColor.WindScale(data.Value); // 0.5f

            spriteBatch.Draw(_stationTex, staionRenderPosition, null, color, 0f, new Vector2(_stationTex.Width / 2f, _stationTex.Height / 2f), scale: stationScale, SpriteEffects.None, 0f);
        }
    }

    public void RegenerateFieldTexture(List<WeatherStation> stations)
    {
        if (stations.Count < 2)
        {
            Console.WriteLine("2 stations are needed for the field");
            return;
        }

        float scaleX = (float)_destRect.Width / _fieldWidth;
        float scaleY = (float)_destRect.Height / _fieldHeight;

        for (int y = 0; y < _fieldHeight; y++)
        {
            for (int x = 0; x < _fieldWidth; x++)
            {
                /*float screenX = _destRect.X + (x * scaleX);
                float screenY = _destRect.Y + (y * scaleY);

                Vector2 p = new Vector2(screenX, screenY);*/
                Vector2 p = new Vector2(x * scaleX, y * scaleY);

                // Before the point p was in screen space, now it is in field space. Less pixels!!!!!

                StationData? blended = InterpolateIDW(p, stations);

                int pixelIndex = y * _fieldWidth + x;

                if (!blended.HasValue)
                {
                    _fieldPixels[pixelIndex] = Color.DarkGray;
                    continue;
                }

                _fieldPixels[pixelIndex] = WeatherColor.ToColor(blended.Value);
            }
        }

        _fieldTex.SetData(_fieldPixels);
    }

    // https://en.wikipedia.org/wiki/Inverse_distance_weighting
    private StationData? InterpolateIDW(Vector2 p, List<WeatherStation> stations)
    {
        float totalWeight = 0f;

        float temperature = 0f;
        float pressure = 0f;
        float humidity = 0f;
        float wind = 0f;

        DateTime latestTime = DateTime.MinValue;

        foreach (WeatherStation station in stations)
        {
            StationData? data = station.GetLatestData();
            if (!data.HasValue) continue;

            Vector2 pos = ScreenToField(station.FieldPosition);

            // Distance from p to the station
            float dx = pos.X - p.X;
            float dy = pos.Y - p.Y;
            float distSq = dx * dx + dy * dy;
            if (distSq <= 0.0001f) return data.Value;
            float weight = 1f / MathF.Sqrt(distSq);

            totalWeight += weight;

            temperature += data.Value.Temperature * weight;
            pressure += data.Value.Pressure * weight;
            humidity += data.Value.Humidity * weight;
            wind += data.Value.WindSpeed * weight;

            if (data.Value.Time > latestTime)
                latestTime = data.Value.Time;
        }

        if (totalWeight <= 0.0001f)
            return null;

        return new StationData(
            Time: latestTime,
            Temperature: temperature / totalWeight,
            Pressure: pressure / totalWeight,
            Humidity: humidity / totalWeight,
            WindSpeed: wind / totalWeight
        );
    }

    private Vector2 FieldToScreen(Vector2 fieldPosition)
    {
        return new Vector2(_destRect.X + fieldPosition.X, _destRect.Y + fieldPosition.Y);
    }

    private Vector2 ScreenToField(Vector2 screenPosition)
    {
        return new Vector2(screenPosition.X - _destRect.X, screenPosition.Y - _destRect.Y);
    }
}