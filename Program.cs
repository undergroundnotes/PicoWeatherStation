using System.Collections.Generic;
using System.Text.Json;
using DataVisualizer;
using DataVisualizer.ServerReading;

// Load Visualizer Settings
string fileJson = "aashjasdjas";

using JsonDocument doc = JsonDocument.Parse(fileJson);
JsonElement root = doc.RootElement;
string serverUrl = root.GetProperty("url").GetString();

WeatherColorSettings colorSettings = new WeatherColorSettings(
    root.GetProperty("temp-min").GetDouble(),
    root.GetProperty("temp-max").GetDouble(),
    root.GetProperty("pres-min").GetDouble(),
    root.GetProperty("pres-max").GetDouble(),
    root.GetProperty("hum-min").GetDouble(),
    root.GetProperty("hum-max").GetDouble(),
    root.GetProperty("wind-min").GetDouble(),
    root.GetProperty("wind-max").GetDouble()
);

// Setup
List<ServerJson> dataBank = new List<ServerJson>();

ServerController serverController = new ServerController(
    new ServerReader(serverUrl),
    dataBank);

using var game = new LiveGame(dataBank, colorSettings);
game.Run();
