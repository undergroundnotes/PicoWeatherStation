using System;
using System.Threading.Tasks;
using DataVisualizer.ServerReading;

namespace DataVisualizerLive.ServerReading;

public class MockReader : IServerReader
{
    private readonly Random _random;

    private float _stationATemperature = 18f;
    private float _stationAHumidity = 55f;
    private float _stationAWind = 12f;
    private float _stationAPressure = 1012f;
    private float _stationBTemperature = 22f;
    private float _stationBHumidity = 48f;
    private float _stationBWind = 8f;
    private float _stationBPressure = 1008f;

    private bool _sendStationA = true;

    public MockReader()
    {
        _random = new Random();
    }

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public async Task<ServerJson> ReadServer()
    {
        await Task.Delay(1000);

        bool isStationA = _sendStationA;
        _sendStationA = !_sendStationA;

        if (isStationA)
        {
            _stationATemperature += RandomDelta(0.4f);
            _stationAHumidity += RandomDelta(1.5f);
            _stationAWind += RandomDelta(1.0f);
            _stationAPressure += RandomDelta(0.8f);

            _stationATemperature = Math.Clamp(_stationATemperature, -20f, 40f);
            _stationAHumidity = Math.Clamp(_stationAHumidity, 0f, 100f);
            _stationAWind = Math.Clamp(_stationAWind, 0f, 80f);
            _stationAPressure = Math.Clamp(_stationAPressure, 950f, 1050f);

            return new ServerJson
            {
                DeviceId = "station_a",
                Serial = 1,
                Time = DateTime.UtcNow,
                Temperature = _stationATemperature,
                Humidity = _stationAHumidity,
                Wind = _stationAWind,
                Pressure = _stationAPressure
            };
        }
        else
        {
            _stationBTemperature += RandomDelta(0.4f);
            _stationBHumidity += RandomDelta(1.5f);
            _stationBWind += RandomDelta(1.0f);
            _stationBPressure += RandomDelta(0.8f);

            _stationBTemperature = Math.Clamp(_stationBTemperature, -20f, 40f);
            _stationBHumidity = Math.Clamp(_stationBHumidity, 0f, 100f);
            _stationBWind = Math.Clamp(_stationBWind, 0f, 80f);
            _stationBPressure = Math.Clamp(_stationBPressure, 950f, 1050f);

            return new ServerJson
            {
                DeviceId = "station_b",
                Serial = 2,
                Time = DateTime.UtcNow,
                Temperature = _stationBTemperature,
                Humidity = _stationBHumidity,
                Wind = _stationBWind,
                Pressure = _stationBPressure
            };
        }
    }

    private float RandomDelta(float magnitude)
    {
        return ((float)_random.NextDouble() * 2f - 1f) * magnitude;
    }
}
