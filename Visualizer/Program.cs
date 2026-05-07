using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text.Json;
using DataVisualizer;
using DataVisualizer.ServerReading;
using DataVisualizerLive.ServerReading;

// Load Visualizer Settings
string settingsFilePath = Path.Combine(AppContext.BaseDirectory, "VisualizerSettings.json");
using JsonDocument doc = JsonDocument.Parse(File.ReadAllText(settingsFilePath));
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
ConcurrentQueue<ServerJson> pendingReadings = new();

ServerController serverController = new ServerController(
    /*new ServerReader(serverUrl),*/
    new MockReader(),
    pendingReadings);

await serverController.ConnectAsync();
serverController.Start();

using var game = new LiveGame(pendingReadings, colorSettings);
game.Run();
