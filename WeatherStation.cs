using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace DataVisualizer;

public class WeatherStation
{
    public string Label { get; set; }
    public Vector2 NormalizedPosition { get; set; }

    // This is always ordered by time. NO! It is ordered by oldest to newest! Time is an illusion
    public readonly List<StationData> StationDatas;// Yes, this is a bad name. I dont care. This contains temp, pres, hum

    public WeatherStation(string label, Vector2 normalPosition, List<StationData> stationDatas = null)
    {
        Label = label;
        NormalizedPosition = normalPosition;
        StationDatas = stationDatas ?? new List<StationData>();

    }

    public void AddReading(StationData data)
    {
        StationDatas.Add(data);
    }

    public bool HasData()
    {
        return StationDatas.Count > 0;
    }

    public StationData? GetLatestData()
    {
        if (StationDatas.Count == 0)
            return null;

        return StationDatas[^1];
    }

    public bool TryGetLatestData(out StationData data)
    {
        if (!HasData())
        {
            data = default;
            return false;
        }

        data = StationDatas[^1];
        return true;
    }

    public string DescribeLatest()
    {
        if (!TryGetLatestData(out StationData data))
            return $"{Label}\nNo readings";

        return $"{Label}\n{data}";
    }

    /*public StationData GetStationDataAtTime(DateTime time)
    {
        return StationDatas[
            ClosestIndexByTime(
                StationDatas,
                time,
                x => x.Time)
        ];
    }*/

    public StationData? GetStationData()
    {
        // always return latest reading??? Since it is live
        return GetLatestData();
    }

    // BST, where givenTime is the query, objTime is the function to get the time from the struct/obj
    // We only care about the fences because we are searching for floats
    /* private int ClosestIndexByTime<T>(List<T> list, DateTime givenTime, Func<T, DateTime> objTime)
     {
         if (list.Count == 1) return 0;
         if (givenTime <= objTime(list[0])) return 0;
         if (givenTime >= objTime(list[list.Count - 1])) return list.Count - 1;

         int low = 0;
         int high = list.Count - 1;
         while (low + 1 < high)
         {
             int mid = low + ((high - low) / 2);
             DateTime time = objTime(list[mid]);

             if (time < givenTime) low = mid;
             else high = mid;
         }

         DateTime floor = objTime(list[low]);
         DateTime ceiling = objTime(list[high]);

         return (givenTime - floor <= ceiling - givenTime) ? low : high;
     }*/
}
