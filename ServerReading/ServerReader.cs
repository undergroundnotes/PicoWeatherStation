using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;

namespace DataVisualizer.ServerReading;

/*
This will be ran on a seperate thread and such.
This is essentially a wrapper for the server test demo
*/
public class ServerReader : IServerReader
{
    private string _url;
    private StreamReader _reader;

    public ServerReader(string url)
    {
        _url = url;
    }

    public async Task InitializeAsync()
    {
        var http = new HttpClient();
        var request = new HttpRequestMessage(HttpMethod.Get, _url);
        request.Headers.Add("Accept", "text/event-stream");

        var response = await http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();
        var stream = await response.Content.ReadAsStreamAsync();
        _reader = new StreamReader(stream);
    }

    public async Task<ServerJson> ReadServer()
    {
        if (_reader == null) throw new InvalidOperationException("ServerReader not initialized.");

        while (true)
        {
            string line = await _reader.ReadLineAsync() ?? throw new Exception("Server Reading was null!!!");

            if (!line.StartsWith("data:")) continue;

            string jsonText = line.Substring("data:".Length).Trim();

            ServerJson? json = JsonSerializer.Deserialize<ServerJson>(jsonText);

            if (!json.HasValue) throw new Exception("Json conversion was null!!!");

            return json.Value;
        }
    }
}