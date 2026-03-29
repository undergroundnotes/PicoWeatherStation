using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DataVisualizer.ServerReading;

namespace DataVisualizerLive.ServerReading;

public class MockReader : IServerReader
{
    private class MockStationState
    {
        public required string DeviceId { get; init; }
        public float Temperature { get; set; }
        public float Humidity { get; set; }
        public float Wind { get; set; }
        public float Pressure { get; set; }
    }

    private const int SCALE = 20;
    private const int DELAY_MAX = 1000;
    private const int DELAY_MIN = 700;

    private readonly Random _random;

    private readonly List<MockStationState> _stations;

    private int _nextStationIndex = 0;
    private int _nextSerial = 1;

    public MockReader(int seed = 6969, int stationCount = 5)
    {
        _random = new Random(seed);
        _stations = new List<MockStationState>();

        for (int i = 0; i < stationCount; i++)
        {
            _stations.Add(new MockStationState
            {
                DeviceId = $"station_{(char)('a' + i)}",
                Temperature = RandomRange(10f, 30f),
                Humidity = RandomRange(30f, 80f),
                Wind = RandomRange(2f, 20f),
                Pressure = RandomRange(990f, 1025f)
            });
        }
    }

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public async Task<ServerJson> ReadServer()
    {
        await Task.Delay(_random.Next(DELAY_MIN, DELAY_MAX));

        MockStationState station = _stations[_nextStationIndex];

        _nextStationIndex++;
        if (_nextStationIndex >= _stations.Count)
            _nextStationIndex = 0;

        station.Temperature += RandomDelta(0.4f);
        station.Humidity += RandomDelta(1.5f);
        station.Wind += RandomDelta(1.0f);
        station.Pressure += RandomDelta(0.8f);

        station.Temperature = Math.Clamp(station.Temperature, -20f, 40f);
        station.Humidity = Math.Clamp(station.Humidity, 0f, 100f);
        station.Wind = Math.Clamp(station.Wind, 0f, 80f);
        station.Pressure = Math.Clamp(station.Pressure, 950f, 1050f);

        return new ServerJson
        {
            DeviceId = station.DeviceId,
            Serial = _nextSerial++,
            Time = DateTime.UtcNow,
            Temperature = station.Temperature,
            Humidity = station.Humidity,
            Wind = station.Wind,
            Pressure = station.Pressure
        };
    }

    private float RandomDelta(float magnitude)
    {
        return ((float)_random.NextDouble() * 2f - 1f) * magnitude * SCALE;
    }

    private float RandomRange(float min, float max)
    {
        return min + ((float)_random.NextDouble() * (max - min));
    }
}
