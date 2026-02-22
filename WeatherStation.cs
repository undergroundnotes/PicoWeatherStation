using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace DataVisualizer;

public class WeatherStation
{
    public string Label { get; set; }
    public Vector2 NormalizedPosition { get; set; }

    // This is always ordered by time
    public readonly List<WeatherData> WeatherData;// Yes, this is a bad name. I dont care. This contains temp, pres, hum
    public readonly List<WindData> WindData;

    public WeatherStation(string label, Vector2 position, List<WeatherData> weatherDatas, List<WindData> windDatas)
    {
        Label = label;
        NormalizedPosition = position;
        WeatherData = weatherDatas;
        WindData = windDatas;
    }

    /* public WeatherData GetReadingAtTime(float simulationTime)
     {
         int index = (int)MathF.Round(simulationTime / _timeStep);
         index = Math.Clamp(index, 0, weatherData.Count - 1);
         return weatherData[index];
     }*/

    public string DescribeAtTime(float simulationTime)
    {
        return $"{Label}\n{GetStationDataAtTime(simulationTime).ToString()}";
    }

    public StationData GetStationDataAtTime(float simulationTime)
    {
        // Get closest data from weather data
        // Get closest data from windData
        // Combine into StationData
        WeatherData weather = WeatherData[ClosestIndexByTime(WeatherData, simulationTime, x => x.time)];

        WindData wind = WindData[ClosestIndexByTime(WindData, simulationTime, x => x.time)];

        return new StationData(
            weatherTimeReading: weather.time,
            WindTimeReading: wind.time,
            temperature: weather.temperature,
            pressure: weather.pressure,
            humidity: weather.humidity,
            windSpeed: wind.windspeed
        );
    }

    // BST, where givenTime is the query, objTime is the function to get the time from the struct/obj
    // We only care about the fences because we are searching for floats
    private int ClosestIndexByTime<T>(List<T> list, float givenTime, Func<T, float> objTime)
    {
        if (list.Count == 1) return 0;
        if (givenTime <= objTime(list[0])) return 0;
        if (givenTime >= objTime(list[list.Count - 1])) return list.Count - 1;

        int low = 0;
        int high = list.Count - 1;
        while (low + 1 < high)
        {
            int mid = low + ((high - low) / 2);
            float time = objTime(list[mid]);

            if (time < givenTime) low = mid;
            else high = mid;
        }

        float floor = objTime(list[low]);
        float ceiling = objTime(list[high]);

        return (givenTime - floor <= ceiling - givenTime) ? low : high;
    }
}
