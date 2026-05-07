var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://0.0.0.0:6767");
var app = builder.Build();

List<HttpResponse> clients = new();

var logWriter = TextWriter.Synchronized(
    new StreamWriter("Output.txt", append: true)
    {
        AutoFlush = true
    });


app.MapPost("/", async (HttpContext context) =>
{
    using var reader = new StreamReader(context.Request.Body);
    string message = await reader.ReadToEndAsync();

    Console.WriteLine("Received: " + message);
    logWriter.WriteLine(message);

    foreach (var client in clients.ToArray())
    {
        try
        {
            // https://www.codemag.com/Article/2309051/Developing-Real-Time-Web-Applications-with-Server-Sent-Events-in-ASP.NET-7-Core
            await client.WriteAsync($"data: {message}\n\n");
            await client.Body.FlushAsync();
        }
        catch
        {
            clients.Remove(client);
        }
    }

    return Results.Ok();
});


// https://roxeem.com/2025/10/24/a-pragmatic-guide-to-server-sent-events-sse-in-asp-net-core/
app.MapGet("/", async (HttpContext context) =>
{
    context.Response.Headers.Append("Content-Type", "text/event-stream");
    context.Response.Headers.Append("Cache-Control", "no-cache");
    context.Response.Headers.Append("Connection", "keep-alive");

    clients.Add(context.Response);

    await context.Response.WriteAsync("connected\n\n");
    await context.Response.Body.FlushAsync();

    try
    {
        await Task.Delay(Timeout.Infinite, context.RequestAborted);
    }
    catch
    {
    }
    finally
    {
        clients.Remove(context.Response);
    }
});

app.Lifetime.ApplicationStopping.Register(() =>
{
    logWriter.Dispose();
});

app.Run();



/*
Received: {"humidity": 39.28, "time": "2026-04-09 20:39:54", "temperature": 21.79, "pressure": 994.82, "wind_speed": 0.3619115, "serial": 52, "device_id": "PICO_WEATHER_03 (Maheep)"}
Received: {"humidity": 39.28, "time": "2026-04-09 20:40:00", "temperature": 21.79, "pressure": 994.83, "wind_speed": 0.3619115, "serial": 53, "device_id": "PICO_WEATHER_03 (Maheep)"}
Received: {"humidity": 39.26, "time": "2026-04-09 20:40:06", "temperature": 21.79, "pressure": 994.85, "wind_speed": 0.3619115, "serial": 54, "device_id": "PICO_WEATHER_03 (Maheep)"}
Received: {"humidity": 39.28, "time": "2026-04-09 20:40:12", "temperature": 21.79, "pressure": 994.88, "wind_speed": 0.3619115, "serial": 55, "device_id": "PICO_WEATHER_03 (Maheep)"}
Received: {"humidity": 39.28, "time": "2026-04-09 20:40:18", "temperature": 21.79, "pressure": 994.8, "wind_speed": 0.3619115, "serial": 56, "device_id": "PICO_WEATHER_03 (Maheep)"}
Received: {"humidity": 39.36, "time": "2026-04-09 20:40:24", "temperature": 21.79, "pressure": 994.93, "wind_speed": 0.3619115, "serial": 57, "device_id": "PICO_WEATHER_03 (Maheep)"}
Received: {"humidity": 39.33, "time": "2026-04-09 20:40:30", "temperature": 21.79, "pressure": 994.89, "wind_speed": 0.3619115, "serial": 58, "device_id": "PICO_WEATHER_03 (Maheep)"}
Received: {"humidity": 39.44, "time": "2026-04-09 20:40:36", "temperature": 21.78, "pressure": 994.87, "wind_speed": 0.3619115, "serial": 59, "device_id": "PICO_WEATHER_03 (Maheep)"}
Received: {"humidity": 39.35, "time": "2026-04-09 20:40:42", "temperature": 21.78, "pressure": 994.89, "wind_speed": 0.3619115, "serial": 60, "device_id": "PICO_WEATHER_03 (Maheep)"}

*/