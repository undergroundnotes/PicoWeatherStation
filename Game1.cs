using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace DataVisualizer;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;

    private MouseState _currentMouseState;
    private SpriteBatch _spriteBatch;

    private SpriteFont font;

    private string weatherDataDirectory = "WeatherData"; // When changing this, make sure to update the csproj
    private string delimiter = "\t";

    List<WeatherStation> weatherStations;

    private string infoLabel = "";

    private float currentTimeInSeconds = 0;
    private float accumulatedTime = 0f;
    private float timeIncrementInterval = 0.6f;
    private float simulationStep = 0.5f;// This needs to be a multiple of the data step inc
    // This implies another assertion, that all datasets have the same step size
    // This doesnt need to be a multiple of the data step inc, if we adjust the rounding formula
    // todo



    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        base.Initialize();

        // 1. Load weather data
        // Take each file inside /WeatherData
        // The number of files is how many samples there are.
        // Read each file and build array
        weatherStations = ConstructWeatherStations();

        // n = 1

        WeatherStation station = weatherStations[0];

        station.Position = new Vector2(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2);

        station.OnEnter += (sender, args) =>
        {
            infoLabel = ((WeatherStation)sender).GenerateStringLabel();
        };

        station.OnExit += (sender, args) =>
        {
            infoLabel = "";
        };

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

        if (accumulatedTime >= timeIncrementInterval)
        {
            currentTimeInSeconds += simulationStep;
            accumulatedTime -= timeIncrementInterval;
        }


        foreach (WeatherStation s in weatherStations)
        {
            s.Update(_currentMouseState, currentTimeInSeconds);
        }

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        _spriteBatch.Begin();

        foreach (WeatherStation s in weatherStations)
        {
            s.DrawStation(_spriteBatch, gameTime);
        }

        _spriteBatch.DrawString(font, infoLabel, new Vector2(8, 8), Color.White);

        //_spriteBatch.DrawString(font, "CurrentTimeInSeconds: " + currentTimeInSeconds, new Vector2((GraphicsDevice.Viewport.Width / 2) - 64, GraphicsDevice.Viewport.Height - 32), Color.White);
        _spriteBatch.DrawString(font, "CurrentTimeInSeconds: " + currentTimeInSeconds, new Vector2(8, 154), Color.White);

        _spriteBatch.End();


        base.Draw(gameTime);
    }


    private List<WeatherStation> ConstructWeatherStations()
    {
        string folderPath = Path.Combine(Directory.GetCurrentDirectory(), weatherDataDirectory);
        System.Console.WriteLine(folderPath);
        string[] files = Directory.GetFiles(folderPath, "*.txt");

        List<WeatherStation> stations = new List<WeatherStation>();

        foreach (string fileName in files)
        {
            System.Console.WriteLine("Processing file: " + fileName);

            List<WeatherData> data = new List<WeatherData>();

            try
            {
                foreach (string lines in File.ReadLines(fileName))
                {
                    string[] tokens = lines.Split(delimiter);

                    float time = float.Parse(tokens[0]);
                    float temp = float.Parse(tokens[1]);
                    float pres = float.Parse(tokens[2]);
                    float hum = float.Parse(tokens[3]);

                    data.Add(new WeatherData(time, temp, pres, hum));
                }

                string cleanName = fileName.Split("\\")[^1];

                stations.Add(new WeatherStation(
                    GraphicsDevice,
                    cleanName,
                    Vector2.One,
                    16,
                    data
                ));

            }
            catch (Exception e)
            {
                throw new FileLoadException("Error reading file: " + fileName + "\n" + e.Message);
                // Probably crash here.
            }
        }

        return stations;
    }
}
