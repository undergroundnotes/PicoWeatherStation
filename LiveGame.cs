using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using DataVisualizer.ServerReading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace DataVisualizer;

public class LiveGame : Game
{
    private GraphicsDeviceManager _graphics;
    private MouseState _currentMouseState;
    private SpriteBatch _spriteBatch;
    private SpriteFont font;

    List<WeatherStation> weatherStations;
    private WeatherMap weatherMap;

    private string infoLabel = "";

    private const float ISO_BAR_STEP = 0.05f;
    private float _calculatedIsoStep;

    private readonly ConcurrentQueue<ServerJson> _pendingReadings;
    private bool _mapDirty = false;
    private int _totalReadingsProcessed = 0;

    private DateTime _currentQueryTime;

    public LiveGame(ConcurrentQueue<ServerJson> pendingReadings, WeatherColorSettings weatherColor)
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        _pendingReadings = pendingReadings;

        WeatherColor.SetValues(weatherColor);

        weatherStations =
        [
            new WeatherStation("station_a", Vector2.One),
            new WeatherStation("station_b", Vector2.One),
        ];
    }

    protected override void Initialize()
    {
        base.Initialize();

        _calculatedIsoStep = (WeatherColor.PRES_MAX - WeatherColor.PRES_MIN) * ISO_BAR_STEP;

        // n =  2
        // TODO: CREATE WEATHER STATIONS MANUALLY. Since I am not doing it automaticallyt
        weatherStations[0].NormalizedPosition = new Vector2(0.25f, 0.50f);
        weatherStations[1].NormalizedPosition = new Vector2(0.85f, 0.50f);

        weatherMap = new WeatherMap(
            GraphicsDevice,
            new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height),
            weatherStations,
            isoBarStep: _calculatedIsoStep,
            stationRadius: 16
        );

        //weatherMap.RegenerateFieldTexture(currentTimeInSeconds, weatherStations[0], weatherStations[1]);
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        font = Content.Load<SpriteFont>("font");
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        _currentMouseState = Mouse.GetState();
        _currentQueryTime = DateTime.UtcNow;


        while (_pendingReadings.TryDequeue(out ServerJson json))
        {
            StationData data = ConvertJson(json);

            // TODO: update station with data
            WeatherStation targetStation = json.DeviceId switch
            {
                "station_a" => weatherStations[0],
                "station_b" => weatherStations[1],
                _ => throw new Exception($"Station '{json.DeviceId}' does not exist!"),
            };
            targetStation.StationDatas.Add(data);

            _totalReadingsProcessed++;
            _mapDirty = true;
        }

        // Render data if valid
        if (_mapDirty && weatherStations.Count >= 2)
        {
            weatherMap.RegenerateFieldTexture(weatherStations[0], weatherStations[1]);

            _mapDirty = false;
        }

        // Mouse Logic is eternal
        WeatherStation hoveredStation = weatherMap.GetStationMouseOverlap(_currentMouseState);
        if (hoveredStation != null)
        {
            infoLabel = hoveredStation.DescribeLatest();
        }
        else if (weatherStations.Count >= 2)
        {
            var blended = weatherMap.GetInterpolatedDataAtMouse(_currentMouseState, weatherStations[0], weatherStations[1]);
            infoLabel = blended.HasValue ? blended.Value.ToString().Replace("Time", "INTERPOLATED-TIME") : "";
        }
        else
        {
            infoLabel = "Waiting for weather station data...";
        }

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        _spriteBatch.Begin();

        weatherMap.Draw(_spriteBatch);

        _spriteBatch.DrawString(font, infoLabel.Replace("\t", "    "), new Vector2(8, 8), Color.White);


        _spriteBatch.DrawString(font,
        $"CurrentTimeInSeconds: {_currentQueryTime:yyyy-MM-dd HH:mm:ss}\nIsobar step ({ISO_BAR_STEP * 100}%): {_calculatedIsoStep}",
        new Vector2(8, 154), Color.White);

        _spriteBatch.End();


        base.Draw(gameTime);
    }

    private StationData ConvertJson(ServerJson json)
    {
        return new StationData(
            Time: json.Time,
            Temperature: json.Temperature,
            Humidity: json.Humidity,
            WindSpeed: json.Wind,
            Pressure: json.Pressure
        );
    }
}
