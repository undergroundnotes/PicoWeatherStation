using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DataVisualizer.ServerReading;

// maybe a way to get a frame of the list? This would avoid race cons. when doing ops.

public class ServerController
{
    private IServerReader _reader;
    private readonly ConcurrentQueue<ServerJson> _pendingReadings;
    private CancellationTokenSource? _cts;

    public ServerController(IServerReader reader, ConcurrentQueue<ServerJson> pendingReadings)
    {
        _reader = reader;
        _pendingReadings = pendingReadings;
    }

    public async Task ConnectAsync()
    {
        await _reader.InitializeAsync();
    }

    public void Start()
    {
        _cts = new CancellationTokenSource();

        Task.Run(() => PollDataAsync(_cts.Token));
    }

    public void Stop()
    {
        _cts?.Cancel();
    }

    private async Task PollDataAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                ServerJson json = await _reader.ReadServer();

                _pendingReadings.Enqueue(json);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Server error: " + ex.Message);
                await Task.Delay(1000, token);
            }
        }
    }
}