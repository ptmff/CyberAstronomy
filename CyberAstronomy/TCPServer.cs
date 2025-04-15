using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;


namespace CyberAstronomy;

public class TCPServer
{
    private readonly TcpListener _listener;
    private readonly ConcurrentDictionary<string, TcpClient> _clients = new ConcurrentDictionary<string, TcpClient>();
    private bool _isRunning = false;

    public TCPServer(string ipAddress, int port)
    {
        _listener = new TcpListener(IPAddress.Parse(ipAddress), port);
    }

    public async Task StartAsync()
    {
        _listener.Start();
        _isRunning = true;
        Console.WriteLine("TCP-сервер запущен.");

        while (_isRunning)
        {
            var client = await _listener.AcceptTcpClientAsync();
            _ = Task.Run(() => HandleClientAsync(client));
        }
    }

    private async Task HandleClientAsync(TcpClient client)
    {
        try
        {
            using (client)
            using (var stream = client.GetStream())
            {
                // Принимаем уникальный идентификатор сессии (строкой)
                byte[] buffer = new byte[256];
                int count = await stream.ReadAsync(buffer, 0, buffer.Length);
                string sessionId = Encoding.UTF8.GetString(buffer, 0, count).Trim();
                if (!string.IsNullOrEmpty(sessionId))
                {
                    _clients.TryAdd(sessionId, client);
                    Console.WriteLine($"Клиент с сессией {sessionId} подключен по TCP.");

                    // Отправляем реальный каталог, например, список серверов (можно расширить)
                    string catalog = "Каталог серверов: ServerA, ServerB, ServerC";
                    byte[] data = Encoding.UTF8.GetBytes(catalog);
                    await stream.WriteAsync(data, 0, data.Length);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Ошибка TCP клиента: " + ex.Message);
        }
    }

    public void Stop()
    {
        _isRunning = false;
        _listener.Stop();
    }
}