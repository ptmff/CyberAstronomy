namespace CyberAstronomy;

public class UdpDataServerBackgroundService : BackgroundService
{
    private readonly UdpDataServer _udpServer;

    public UdpDataServerBackgroundService()
    {
        // Задайте IP-адрес и порт для UDP-сервера (пример: 127.0.0.1:8001)
        _udpServer = new UdpDataServer("127.0.0.1", 8001);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Передаем токен отмены, если понадобится добавить соответствующую обработку в методе сервера
        await _udpServer.StartSendingDataAsync();
    }
}