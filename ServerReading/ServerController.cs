using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DataVisualizer.ServerReading;

public class ServerController
{
    private IServerReader _reader;
    private List<ServerJson> _dataBank;

    public ServerController(IServerReader reader, List<ServerJson> dataBank)
    {
        _reader = reader;
        _dataBank = dataBank;
    }

    public void Connect()
    {
        _reader.Initialize();
    }

    // This function should be multi Threaded
    public async Task Run()
    {
        // I cannot use a seperate thread. Well, I can. Regardless, EVERYTHING needs to be done through async
    }

    private async Task PollDataAsync()
    {
        while (true)
        {
            ServerJson json = await _reader.ReadServer();
            _dataBank.Add(json);
        }
    }

    // This function should be somewhere else. Its not needed here
    private StationData ConvertJson(ServerJson json)
    {
        return new StationData(
            Time: 0, // TODO: change to date time
            Temperature: json.Temperature,
            Humidity: json.Humidity,
            WindSpeed: json.Wind,
            Pressure: json.Pressure
        );
    }
}