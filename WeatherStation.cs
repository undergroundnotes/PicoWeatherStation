using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace DataVisualizer;

public class WeatherStation
{
    // TODO: remove current reading and fully detach this from time
    public WeatherData CurrentReading
    {
        get
        {
            return weatherData[currentWeatherDataIndex];
        }
    }

    public string Label { get; set; }

    public Vector2 NormalizedPosition { get; set; }// todo: make this a normalized position detached from render position

    public EventHandler OnEnter;
    public EventHandler OnExit;
    private List<WeatherData> weatherData; // This is always ordered by time
    private int currentWeatherDataIndex = 0;

    private readonly float _timeStep;


    public WeatherStation(string label, Vector2 position, List<WeatherData> data)
    {
        Label = label;
        NormalizedPosition = position;

        weatherData = data;

        _timeStep = data[1].time - data[0].time;
    }

    public string GenerateStringLabel()
    {
        return $"{Label}\nIndex: {currentWeatherDataIndex}\n" + CurrentReading.ToString();
    }

    public WeatherData GetReadingAtTime(float simulationTime)
    {
        int index = (int)MathF.Round(simulationTime / _timeStep);
        index = Math.Clamp(index, 0, weatherData.Count - 1);
        return weatherData[index];
    }

    public string DescribeAtTime(float simulationTime)
    {
        return $"{Label}\n{GetReadingAtTime(simulationTime)}";
    }
}
