using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using DataVisualizer.ServerReading;
using DataVisualizerLive;
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

    private const float ISO_BAR_STEP = 0.15f;
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
    }

    protected override void Initialize()
    {
        base.Initialize();

        Rectangle screenSize = new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);

        _calculatedIsoStep = (WeatherColor.PRES_MAX - WeatherColor.PRES_MIN) * ISO_BAR_STEP;

        weatherMap = new WeatherMap(
            GraphicsDevice,
            screenSize,
            isoBarStep: _calculatedIsoStep,
            stationRadius: 16
        );


        weatherStations =
        [
            new WeatherStation("station_a", new Vector2(0.15f, 0.25f), screenSize),
            new WeatherStation("station_b", new Vector2(0.50f, 0.15f), screenSize),
            new WeatherStation("station_c", new Vector2(0.85f, 0.30f), screenSize),
            new WeatherStation("station_d", new Vector2(0.25f, 0.75f), screenSize),
            new WeatherStation("station_e", new Vector2(0.75f, 0.80f), screenSize),
            new WeatherStation("station_f", new Vector2(0.5f, 0.55f), screenSize)
        ];
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
            StationData data = Utils.ConvertJson(json);

            // TODO: update station with data
            WeatherStation targetStation = json.DeviceId switch
            {
                "station_a" => weatherStations[0],
                "station_b" => weatherStations[1],
                "station_c" => weatherStations[2],
                "station_d" => weatherStations[3],
                "station_e" => weatherStations[4],
                "station_f" => weatherStations[5],
                _ => throw new Exception($"Station '{json.DeviceId}' does not exist!"),
            };
            targetStation.StationDatas.Add(data);

            _totalReadingsProcessed++;
            _mapDirty = true;
        }

        // Render data if valid
        if (_mapDirty && weatherStations.Count >= 2)
        {
            weatherMap.RegenerateFieldTexture(weatherStations);

            _mapDirty = false;
        }

        // Mouse Logic is eternal
        WeatherStation hoveredStation = weatherMap.GetStationMouseOverlap(_currentMouseState, weatherStations);
        if (hoveredStation != null)
        {
            infoLabel = hoveredStation.DescribeLatest();
        }
        else if (weatherStations.Count >= 2)
        {
            var blended = weatherMap.GetInterpolatedDataAtMouse(_currentMouseState, weatherStations);
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

        weatherMap.Draw(_spriteBatch, weatherStations);

        _spriteBatch.DrawString(font, infoLabel.Replace("\t", "    "), new Vector2(8, 8), Color.White);


        _spriteBatch.DrawString(font,
        $"CurrentTimeInSeconds: {_currentQueryTime:yyyy-MM-dd HH:mm:ss}\nIsobar step ({ISO_BAR_STEP * 100}%): {_calculatedIsoStep}",
        new Vector2(8, 154), Color.White);

        _spriteBatch.End();


        base.Draw(gameTime);
    }
}
