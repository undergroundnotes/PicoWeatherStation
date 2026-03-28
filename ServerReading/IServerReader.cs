using System.Threading.Tasks;

namespace DataVisualizer.ServerReading;

public interface IServerReader
{
    Task<ServerJson> ReadServer();

    Task InitializeAsync();
}