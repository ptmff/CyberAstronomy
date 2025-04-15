using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace CyberAstronomy;

public class TCPServerBackgroundService : BackgroundService
{
    private TCPServer _tcpServer;

    public TCPServerBackgroundService()
    {
        // Задайте IP и порт для TCP-сервера
        _tcpServer = new TCPServer("127.0.0.1", 8000);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _tcpServer.StartAsync();
    }
}