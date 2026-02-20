using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace DataVisualizer;

public class WeatherStation
{
    public WeatherData CurrentReading
    {
        get
        {
            return weatherData[currentWeatherDataIndex];
        }
    }

    public string Label { get; set; }

    public Vector2 Position { get; set; }

    // Updating these fields will require to regenerate the texture, which is bad
    private int _radius;

    private Texture2D texture;

    public EventHandler OnEnter;
    public EventHandler OnExit;
    private List<WeatherData> weatherData; // This is always ordered by time
    private int currentWeatherDataIndex = 0;

    private readonly float _timeStep;

    // State to later remove
    private bool insidePrev = false;
    private float _previousSimTime = -1;


    public WeatherStation(GraphicsDevice graphicsDevice, string label, Vector2 position, int radius, List<WeatherData> data)
    {
        Label = label;
        Position = position;
        _radius = radius;

        weatherData = data;

        texture = CreateCircleTexture(graphicsDevice, _radius, Color.White);

        _timeStep = data[1].time - data[0].time;
    }

    public void DrawStation(SpriteBatch sb, GameTime time)
    {
        Color calculatedColor = WeatherColor.ToColor(weatherData[currentWeatherDataIndex]);

        sb.Draw(texture, Position, null, calculatedColor, 0f, new Vector2(_radius, _radius), 1, SpriteEffects.None, 0); // white * color = color
    }

    public void Update(MouseState mouseState, float simulationTime)
    {
        bool insideNow = IsInArea(new Vector2(mouseState.Position.X, mouseState.Position.Y));


        // Make sure, our current weatherData matches the time
        /*
        Case 1: sim Time is less than current index time
        Case 2: Sim time is greater than current index time
        Case 3: sim time matches current index time

        Note: time is a float, thus we need to modify case 3
        Case 3: sime times matches current index time within small range (0.001)

        Overall, I think I need some sort of wiggly room. However, all the edge cases which come to my mind only arise
        when things arnt perfect.

        Should we assume the sim time will always match up with an index? as in, the step increment on the sim timer, always matches the step increment on the data?
        
        Ok: Assume the sim time step increment is some multiple of the datas step increment
        Speed is controlled by the simulation rate.
        In theory, we will always have the step increment match the data

        // I assert that the simulation step time is a multiple n of the data step time.
        // This means, when n=1, we cycle through every entry; n=2, every second, etc. 

        */

        if (float.Abs(_previousSimTime - simulationTime) > 0.001f)
        {
            // update index
            int index = (int)MathF.Round(simulationTime / _timeStep);
            currentWeatherDataIndex = Math.Clamp(index, 0, weatherData.Count - 1);
        }

        // hover
        if ((insidePrev && insideNow) && _previousSimTime != simulationTime)
        {
            // if the simTime is different call the OnEnter, as this is new
            OnEnter?.Invoke(this, null);
        }
        // Exit: previous was in, now out
        else if (insidePrev && !insideNow)
        {
            OnExit?.Invoke(this, null);
        }
        // Enter: previous was out, now in
        else if (!insidePrev && insideNow)
        {
            OnEnter?.Invoke(this, null);
        }


        insidePrev = insideNow;
        _previousSimTime = simulationTime;
    }

    private bool IsInArea(Vector2 point)
    {
        return Vector2.Distance(Position, point) < _radius;
    }

    public string GenerateStringLabel()
    {
        return $"{Label}\nIndex: {currentWeatherDataIndex}\n" + CurrentReading.ToString();
    }

    // I could also have this function outside this class, thus I wouldnt need to be passing around grapgDev
    // However, doing so would actually be more complicated. As the texture is fully based off the radius (and color)
    // Which in theory should be controlled from this class....because of the hitbox....however, If I go more towards
    // a proper code structure, and use components instead of objects.... then this would need to be removed from this
    private Texture2D CreateCircleTexture(GraphicsDevice graphicsDevice, int radius, Color color)
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

                if (distance <= radius)
                    data[y * diameter + x] = color;
                else
                    data[y * diameter + x] = Color.Transparent;
            }
        }

        texture.SetData(data);
        return texture;
    }
}
