using System;
using System.Text.Json.Serialization;

namespace DataVisualizer.ServerReading;

public struct ServerJson
{
    [JsonPropertyName("device_id")]
    public required readonly string DeviceId { get; init; }

    [JsonPropertyName("serial")]
    public required readonly int Serial { get; init; }

    [JsonPropertyName("temperature")]
    public required readonly float Temperature { get; init; }

    [JsonPropertyName("humidity")]
    public required readonly float Humidity { get; init; }

    [JsonPropertyName("pressure")]
    public required readonly float Pressure { get; init; }

    [JsonPropertyName("wind_speed")]
    public required readonly float Wind { get; init; }

    [JsonPropertyName("time")]
    public required readonly DateTime Time { get; init; }
}

/*
    payload = {
        "device_id": DEVICE_ID,
        #"api_key": API_KEY,
        "serial": serial,
        "temperature": temp,
        "humidity": hum,
        "pressure": press,
        "wind_speed": wind,
        "time": tstr
    }
*/