using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace DataVisualizer;

public class WeatherStation
{
    public string Label { get; set; }

    public readonly List<StationData> StationDatas;// Yes, this is a bad name. I dont care. This contains temp, pres, hum
    public bool HasData => StationDatas.Count > 0;

    public Vector2 FieldPosition { get; set; }// Using this is a MASSIVE performance gain. As now ALL the "pixel areas" I am looking at are big. Before when using screen space, I would be doing extra calculations as the field pixel is bigger, thus contains many screen pixels, thus more calcs. However, now THINGS ARE SOO GOOD WTF

    public WeatherStation(string label, Vector2 fieldPosition, List<StationData> stationDatas = null)
    {
        // It is arguable if passing this a fieldPosition makes sense or not, choosing to not do some other thing.
        // Nah, it does make sense
        Label = label;
        FieldPosition = fieldPosition;
        StationDatas = stationDatas ?? new List<StationData>();
    }

    public void AddReading(StationData data)
    {
        StationDatas.Add(data);
    }

    public StationData? GetLatestData()
    {
        if (!HasData)
            return null;

        return StationDatas[^1];
    }

    public StationData? GetStationData()
    {
        return GetLatestData();
    }

    public bool TryGetLatestData(out StationData data)
    {
        if (!HasData)
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
}
