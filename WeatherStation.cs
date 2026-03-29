using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace DataVisualizer;

// Positions are fixed. THis keeps things sane
// Calculations are now based off screenPos, however, normal pos is good for placing the stations
public class WeatherStation
{
    public string Label { get; set; }
    public Vector2 NormalizedPosition { get; }

    public readonly List<StationData> StationDatas;// Yes, this is a bad name. I dont care. This contains temp, pres, hum
    public bool HasData => StationDatas.Count > 0;


    private Vector2 _screenPos = -Vector2.One;
    public Vector2 ScreenPosition => _screenPos;

    public WeatherStation(string label, Vector2 normalPosition, Rectangle screen, List<StationData> stationDatas = null)
    {
        Label = label;
        NormalizedPosition = normalPosition;
        StationDatas = stationDatas ?? new List<StationData>();

        _screenPos = new Vector2(screen.X + (NormalizedPosition.X * screen.Width), screen.Y + (NormalizedPosition.Y * screen.Height));
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
