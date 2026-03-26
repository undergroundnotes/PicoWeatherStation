using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace DataVisualizer;

public class WeatherStation
{
    public string Label { get; set; }
    public Vector2 NormalizedPosition { get; set; }

    // This is always ordered by time
    public readonly List<StationData> StationDatas;// Yes, this is a bad name. I dont care. This contains temp, pres, hum

    public WeatherStation(string label, Vector2 position, List<StationData> stationDatas)
    {
        Label = label;
        NormalizedPosition = position;
        StationDatas = stationDatas;
        
    }
    public string DescribeAtTime(float simulationTime)
    {
        return $"{Label}\n{GetStationDataAtTime(simulationTime).ToString()}";
    }

    public StationData GetStationDataAtTime(float simulationTime)
    {
        // Get closest data
        return StationDatas[ClosestIndexByTime(StationDatas, simulationTime, x => x.Time)];
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
