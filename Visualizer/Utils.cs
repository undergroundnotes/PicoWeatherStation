using DataVisualizer;
using DataVisualizer.ServerReading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DataVisualizerLive;

public static class Utils
{
    public static StationData ConvertJson(ServerJson json)
    {
        return new StationData(
            Time: json.Time,
            Temperature: json.Temperature,
            Humidity: json.Humidity,
            WindSpeed: json.Wind,
            Pressure: json.Pressure
        );
    }

    public static Texture2D CreateCircleTexture(GraphicsDevice graphicsDevice, int radius, Color fillColor, int stroke, Color strokeColor)
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
