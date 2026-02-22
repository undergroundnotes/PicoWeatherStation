using System;
using System.Collections.Generic;
using System.IO;
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
    // For now I will assert that the wind data is the same file with _wind at the end
    private string delimiter = "\t";

    List<WeatherStation> weatherStations;

    private WeatherMap weatherMap;

    private string infoLabel = "";

    private float currentTimeInSeconds = 0;
    private float accumulatedTime = 0f;
    private float timeIncrementInterval = 0.5f;
    private float simulationStep = 0.05f;// This needs to be a multiple of the data step inc
    // This implies another assertion, that all datasets have the same step size
    // This doesnt need to be a multiple of the data step inc, if we adjust the rounding formula
    // todo
    private float _lastRenderedTime = float.NegativeInfinity;

    private const float ISO_BAR_STEP = 0.05f;
    private float _calculatedIsoStep;

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

        // Calculate color min max values
        WeatherColor.SetRanges(weatherStations);

        _calculatedIsoStep = (WeatherColor.PRES_MAX - WeatherColor.PRES_MIN) * ISO_BAR_STEP;

        // n = 2
        weatherStations[0].NormalizedPosition = new Vector2(0.25f, 0.50f);
        weatherStations[1].NormalizedPosition = new Vector2(0.85f, 0.65f);

        weatherMap = new WeatherMap(
            GraphicsDevice,
            new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height),
            weatherStations,
            isoBarStep: ISO_BAR_STEP,
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

        WeatherStation hoveredStation = weatherMap.GetStationMouseOverlap(_currentMouseState);
        if (hoveredStation != null)
        {
            infoLabel = hoveredStation.DescribeAtTime(currentTimeInSeconds);
        }
        else
        {
            var blended = weatherMap.GetInterpolatedDataAtMouse(
                _currentMouseState,
                currentTimeInSeconds,
                weatherStations[0],
                weatherStations[1]);

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

    private List<WeatherStation> ConstructWeatherStations()
    {
        // Ok. I will assert that any data file named `x.txt`, aslo has an `x_wind.txt` for its wind data.

        // Thus, I will need to do 2 passes for constructing the data
        // First pass, avoid the _wind, second, only the _wind
        // no. First we split the lists loop through first, while searching the second

        string folderPath = Path.Combine(Directory.GetCurrentDirectory(), weatherDataDirectory);
        System.Console.WriteLine(folderPath);
        string[] allFiles = Directory.GetFiles(folderPath, "*.txt");
        List<string> weatherDataFiles = new List<string>(allFiles.Length / 2);
        LinkedList<string> windDataFiles = new LinkedList<string>();

        // This is on init; I dont care about speed
        foreach (string file in allFiles)
        {
            if (file.Contains("_wind"))
                windDataFiles.AddLast(file);
            else
                weatherDataFiles.Add(file);
        }
        if (weatherDataFiles.Count != windDataFiles.Count) throw new Exception("Data files are not fully matched");

        List<WeatherStation> stations = new List<WeatherStation>(weatherDataFiles.Count);

        foreach (string file in weatherDataFiles)
        {
            System.Console.WriteLine("Processing file: " + file);
            List<WeatherData> weatherDatas = new List<WeatherData>();
            List<WindData> windDatas = new List<WindData>();

            try
            {
                // 1 Read normal data file based off loop index
                foreach (string lines in File.ReadLines(file))
                {
                    string[] tokens = lines.Split(delimiter);

                    float time = float.Parse(tokens[0]);
                    float temp = float.Parse(tokens[1]);
                    float pres = float.Parse(tokens[2]);
                    float hum = float.Parse(tokens[3]);

                    weatherDatas.Add(new WeatherData(time, temp, pres, hum));
                }

                //2 Read _wind.txt file
                // Loop through linked list to get node, then remove it after, so we are iterating over it again
                string baseName = Path.GetFileNameWithoutExtension(file);
                string expectedWindNameNoExt = baseName + "_wind";

                LinkedListNode<string> node = windDataFiles.First;
                LinkedListNode<string> matchNode = null;

                while (node != null)
                {
                    string windFile = node.Value;
                    string windNameNoExt = Path.GetFileNameWithoutExtension(windFile);

                    if (string.Equals(windNameNoExt, expectedWindNameNoExt, StringComparison.OrdinalIgnoreCase))
                    {
                        matchNode = node;
                        break;
                    }

                    node = node.Next;
                }

                if (matchNode == null) throw new FileNotFoundException("Missing wind file for: " + file);

                string matchedWindFile = matchNode.Value;

                // Read wind file
                foreach (string line in File.ReadLines(matchedWindFile))
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    string[] tokens = line.Split(delimiter);

                    float time = float.Parse(tokens[0]);
                    float wind = float.Parse(tokens[1]);

                    windDatas.Add(new WindData(time, wind));
                }

                windDataFiles.Remove(matchNode);// remove node, so next search is faster


                // Lastely, Construct object
                WeatherStation station = new WeatherStation(
                    baseName,
                    Vector2.One,
                    weatherDatas,
                    windDatas
                );

                stations.Add(station);
            }
            catch (Exception e)
            {
                throw new FileLoadException("Error reading file: " + file + "\n" + e.Message);
                // Probably crash here.
            }
        }

        return stations;
    }




    /*private List<WeatherStation> ConstructWeatherStations()
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

                WeatherStation station = new WeatherStation(
                    cleanName,
                    Vector2.One,
                    data
                );

                stations.Add(station);
            }
            catch (Exception e)
            {
                throw new FileLoadException("Error reading file: " + fileName + "\n" + e.Message);
                // Probably crash here.
            }
        }

        return stations;
    }*/
}
