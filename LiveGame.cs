using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
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

    private List<WeatherStation> _weatherStations;
    private WeatherMap weatherMap;

    private Texture2D _stationTexture;

    private Texture2D _mapTexture;


    private string infoLabel = "";

    private const float ISO_BAR_STEP = 0.05f;
    private float _calculatedIsoStep;

    private readonly ConcurrentQueue<ServerJson> _pendingReadings;
    private bool _mapDirty = false;


    private DateTime _currentQueryTime;

    private MouseState _previousMouseState;
    private WeatherStation _draggedStation;


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
            scale: 0.7f,
            stationTexture: _stationTexture,
            mapAlpha: 0.5f
        );


        _weatherStations =
        [
            new WeatherStation("PICO_WEATHER_01", new Vector2(0.15f * screenSize.Width, 0.25f * screenSize.Height)),
            new WeatherStation("PICO_WEATHER_02", new Vector2(0.50f* screenSize.Width, 0.15f* screenSize.Height)),
            new WeatherStation("PICO_WEATHER_03", new Vector2(0.85f* screenSize.Width, 0.30f* screenSize.Height)),
            new WeatherStation("PICO_WEATHER_04", new Vector2(0.25f* screenSize.Width, 0.75f* screenSize.Height)),
            new WeatherStation("PICO_WEATHER_05", new Vector2(0.75f* screenSize.Width, 0.80f* screenSize.Height)),
            new WeatherStation("station_f", new Vector2(0.5f* screenSize.Width, 0.55f* screenSize.Height))
        ];
        // Pos assignments are ugly ONLY at the start. Beauty comes from time.
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        font = Content.Load<SpriteFont>("font");
        _stationTexture = Content.Load<Texture2D>("picoW");
        _mapTexture = Content.Load<Texture2D>("map");
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        _previousMouseState = _currentMouseState;
        _currentMouseState = Mouse.GetState();

        bool updateMap = true;

        // Drag = held down
        // Thus start = pressed; stop = released; in between: follow the mouse
        // Note: dragging is super fucking slow. I may need to pause the map updates
        if (_currentMouseState.LeftButton == ButtonState.Pressed && _previousMouseState.LeftButton == ButtonState.Released)
        {
            _draggedStation = weatherMap.GetStationMouseOverlap(_currentMouseState, _weatherStations);
        }
        else if (_draggedStation != null && _currentMouseState.LeftButton == ButtonState.Pressed)
        {
            // The position becomes the field of the mouses screen position
            Vector2 fieldPosition = weatherMap.ScreenToField(new Vector2(_currentMouseState.X, _currentMouseState.Y));

            fieldPosition.X = Math.Clamp(fieldPosition.X, 0f, weatherMap.DestRect.Width);
            fieldPosition.Y = Math.Clamp(fieldPosition.Y, 0f, weatherMap.DestRect.Height);

            _draggedStation.FieldPosition = fieldPosition;

            _mapDirty = true;
            updateMap = false;
        }
        else if (_currentMouseState.LeftButton == ButtonState.Released && _previousMouseState.LeftButton == ButtonState.Pressed)
        {
            _draggedStation = null;
        }


        if (updateMap)
        {
            _currentQueryTime = DateTime.UtcNow;
            while (_pendingReadings.TryDequeue(out ServerJson json))
            {
                StationData data = Utils.ConvertJson(json);

                WeatherStation targetStation = GetTargetStation(json.DeviceId, _weatherStations);

                targetStation.StationDatas.Add(data);

                _mapDirty = true;
            }

            // Render data if valid
            if (_mapDirty && _weatherStations.Count >= 2)
            {
                weatherMap.RegenerateFieldTexture(_weatherStations);
                _mapDirty = false;
            }
        }

        // Mouse Logic is eternal. This is dependant on the map
        WeatherStation hoveredStation = weatherMap.GetStationMouseOverlap(_currentMouseState, _weatherStations);
        if (hoveredStation != null)
        {
            infoLabel = hoveredStation.DescribeLatest();
        }
        else if (_weatherStations.Count >= 2)
        {
            var blended = weatherMap.GetInterpolatedDataAtMouse(_currentMouseState, _weatherStations);
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

        _spriteBatch.Begin(/*blendState: _multiplyBlend*/);

        _spriteBatch.Draw(_mapTexture, new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height), Color.White);
        weatherMap.DrawMap(_spriteBatch);

        _spriteBatch.End();

        _spriteBatch.Begin();

        weatherMap.DrawStations(_spriteBatch, _weatherStations);

        _spriteBatch.DrawString(font, infoLabel.Replace("\t", "    "), new Vector2(8, 8), Color.Black);


        _spriteBatch.DrawString(font,
        $"CurrentTimeInSeconds: {_currentQueryTime:yyyy-MM-dd HH:mm:ss}\nIsobar step ({ISO_BAR_STEP * 100}%): {_calculatedIsoStep}",
        new Vector2(8, 154), Color.Black);

        _spriteBatch.End();


        base.Draw(gameTime);
    }

    private readonly BlendState _multiplyBlend = new BlendState
    {
        ColorSourceBlend = Blend.DestinationColor,
        ColorDestinationBlend = Blend.Zero,
        ColorBlendFunction = BlendFunction.Add,

        AlphaSourceBlend = Blend.One,
        AlphaDestinationBlend = Blend.Zero,
        AlphaBlendFunction = BlendFunction.Add
    };

    private WeatherStation GetOrAssignStation(string actualLabel, string baseLabel, List<WeatherStation> weatherStations)
    {
        // Allows for suffixs on deviceId:station.label.
        // This will check with contains, then update the station to be the exact station.
        // Yes, this is technically hardcoded.
        // Why would there ever be percedual unknown weather stations??? Ask you self that question before thinking to deeply into this smelly slopa
        WeatherStation? exactMatch = weatherStations.FirstOrDefault(s => s.Label == actualLabel);

        if (exactMatch != null) return exactMatch;

        WeatherStation station = weatherStations.First(s => s.Label.Contains(baseLabel));

        station.Label = actualLabel;

        return station;
    }

    private WeatherStation GetTargetStation(string deviceId, List<WeatherStation> weatherStations)
    {

        return deviceId switch
        {
            string id when id.Contains("PICO_WEATHER_01") => GetOrAssignStation(id, "PICO_WEATHER_01", weatherStations),
            string id when id.Contains("PICO_WEATHER_02") => GetOrAssignStation(id, "PICO_WEATHER_02", weatherStations),
            string id when id.Contains("PICO_WEATHER_03") => GetOrAssignStation(id, "PICO_WEATHER_03", weatherStations),
            string id when id.Contains("PICO_WEATHER_04") => GetOrAssignStation(id, "PICO_WEATHER_04", weatherStations),
            string id when id.Contains("PICO_WEATHER_05") => GetOrAssignStation(id, "PICO_WEATHER_05", weatherStations),
            string id when id.Contains("PICO_WEATHER_06") => GetOrAssignStation(id, "PICO_WEATHER_06", weatherStations),
            _ => throw new Exception($"Station '{deviceId}' does not exist!"),
        };
    }
}
