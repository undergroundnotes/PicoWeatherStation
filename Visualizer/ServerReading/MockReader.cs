using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DataVisualizer;
using DataVisualizer.ServerReading;

namespace DataVisualizerLive.ServerReading;

// Note: this is broken.
// When switching to read data, we changed station names aswell as how time is done.
// This shouldnt be a too big of fix. This file was originally created using basic assumption (hardcoded) that no longer exist
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

    private const int SCALE = 1;
    private const int DELAY_MAX = 1000;
    private const int DELAY_MIN = 700;

    private readonly Random _random;

    private readonly List<MockStationState> _stations;

    private int _nextStationIndex = 0;
    private int _nextSerial = 1;
    private float _time;

    private float _tempDelta;
    private float _humDelta;
    private float _windDelta;
    private float _presDelta;

    public MockReader(int seed = 6969, int stationCount = 5)
    {
        _random = new Random(seed);
        _stations = new List<MockStationState>();

        for (int i = 0; i < stationCount; i++)
        {
            _stations.Add(new MockStationState
            {
                DeviceId = $"PICO_WEATHER_{(i + 1):D2}",
                Temperature = RandomRange(WeatherColor.TEMP_MIN, WeatherColor.TEMP_MAX) * SCALE,
                Humidity = RandomRange(WeatherColor.HUM_MIN, WeatherColor.HUM_MAX) * SCALE,
                Wind = RandomRange(WeatherColor.WIND_MIN, WeatherColor.WIND_MAX) * SCALE,
                Pressure = RandomRange(WeatherColor.PRES_MIN, WeatherColor.PRES_MAX) * SCALE
            });
        }

        float maxChangePercent = 0.2f;
        _tempDelta = (WeatherColor.TEMP_MAX - WeatherColor.TEMP_MIN) * maxChangePercent * 1.25f * SCALE;
        _humDelta = (WeatherColor.HUM_MAX - WeatherColor.HUM_MIN) * maxChangePercent * 1.5f * SCALE;
        _windDelta = (WeatherColor.WIND_MAX - WeatherColor.WIND_MIN) * maxChangePercent * SCALE;
        _presDelta = (WeatherColor.PRES_MAX - WeatherColor.PRES_MIN) * maxChangePercent * 2f * SCALE;
    }

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public async Task<ServerJson> ReadServer()
    {
        await Task.Delay(_random.Next(DELAY_MIN, DELAY_MAX));

        _time += 0.15f;

        MockStationState station = _stations[_nextStationIndex];

        _nextStationIndex++;
        if (_nextStationIndex >= _stations.Count)
            _nextStationIndex = 0;

        station.Temperature += RandomDelta(_tempDelta);
        station.Humidity += RandomDelta(_humDelta);
        station.Wind += RandomDelta(_windDelta);
        station.Pressure += RandomDelta(_presDelta);

        station.Temperature = Math.Clamp(station.Temperature, WeatherColor.TEMP_MIN - (_tempDelta * 5f), WeatherColor.TEMP_MAX + (_tempDelta * 5f));
        station.Humidity = Math.Clamp(station.Humidity, WeatherColor.HUM_MIN - (_humDelta * 5f), WeatherColor.HUM_MAX + (_humDelta * 5f));
        station.Wind = Math.Clamp(station.Wind, WeatherColor.WIND_MIN - (_windDelta * 5f), WeatherColor.WIND_MAX + (_windDelta * 5f));
        station.Pressure = Math.Clamp(station.Pressure, WeatherColor.PRES_MIN - (_presDelta * 5f), WeatherColor.PRES_MAX + (_presDelta * 5f));

        return new ServerJson
        {
            DeviceId = station.DeviceId,
            Serial = _nextSerial++,
            RawTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
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
