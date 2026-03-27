using System;
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

    private string weatherDataDirectory = "WeatherData"; // When changing this, make sure to update the csproj
    // For now I will assert that the wind data is the same file with _wind at the end
    private string delimiter = "\t";

    List<WeatherStation> weatherStations;

    private WeatherMap weatherMap;

    private string infoLabel = "";

    private float currentTimeInSeconds = 0;
    private float accumulatedTime = 0f;
    private float timeIncrementInterval = 0.5f;
    private float simulationStep = 0.5f;// This needs to be a multiple of the data step inc
    // This implies another assertion, that all datasets have the same step size
    // This doesnt need to be a multiple of the data step inc, if we adjust the rounding formula
    // todo
    private float _lastRenderedTime = float.NegativeInfinity;

    private const float ISO_BAR_STEP = 0.05f;
    private float _calculatedIsoStep;

    private IReadOnlyList<ServerJson> _dataBank;
    private int readingCount = 0;

    public LiveGame(List<ServerJson> dataBank, WeatherColorSettings weatherColor)
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        
        // THIS LIST/DATA NEEDS TO BE A REFERENCE.
        // Same copy which is loaded async
        _dataBank = dataBank;
        readingCount = 0;

        WeatherColor.SetValues(weatherColor);
    }

    protected override void Initialize()
    {
        base.Initialize();

        _calculatedIsoStep = (WeatherColor.PRES_MAX - WeatherColor.PRES_MIN) * ISO_BAR_STEP;

        // n =  

        weatherMap = new WeatherMap(
            GraphicsDevice,
            new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height),
            weatherStations,
            isoBarStep: _calculatedIsoStep,
            stationRadius: 16
        );

        weatherMap.RegenerateFieldTexture(currentTimeInSeconds, weatherStations[0], weatherStations[1]);
        _lastRenderedTime = currentTimeInSeconds;
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
        float deltaSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;
        accumulatedTime += deltaSeconds;

        // # 1 Update readings
        if(_dataBank.Count != readingCount)
        {
            // There has been new readings
            // Get new data
            // Convert data
            
            
            // Render data if valid
            if (accumulatedTime >= timeIncrementInterval)
            {
                currentTimeInSeconds += simulationStep;
                accumulatedTime -= timeIncrementInterval;
            }

            if (currentTimeInSeconds != _lastRenderedTime)
            {
                weatherMap.RegenerateFieldTexture(currentTimeInSeconds, weatherStations[0], weatherStations[1]);
                _lastRenderedTime = currentTimeInSeconds;
            }
        }

        // Mouse Logic is eternal
        WeatherStation hoveredStation = weatherMap.GetStationMouseOverlap(_currentMouseState);
        if (hoveredStation != null)
        {
            infoLabel = hoveredStation.DescribeAtTime(currentTimeInSeconds);
        }
        else
        {
            var blended = weatherMap.GetInterpolatedDataAtMouse(_currentMouseState, currentTimeInSeconds, weatherStations[0], weatherStations[1]);
            infoLabel = blended.HasValue ? blended.Value.ToString() : "";
        }

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        _spriteBatch.Begin();

        weatherMap.Draw(_spriteBatch, currentTimeInSeconds);

        _spriteBatch.DrawString(font, infoLabel.Replace("\t", "    "), new Vector2(8, 8), Color.White);


        _spriteBatch.DrawString(font,
        "CurrentTimeInSeconds: " + Math.Round(currentTimeInSeconds, 3) + $"\nIsobar step ({ISO_BAR_STEP * 100}%): {_calculatedIsoStep}",
        new Vector2(8, 154), Color.White);

        _spriteBatch.End();


        base.Draw(gameTime);
    }
}
