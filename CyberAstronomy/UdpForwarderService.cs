using System.Net.Sockets;
using System.Text;
using Microsoft.AspNetCore.SignalR;

namespace CyberAstronomy;

public class UdpForwarderService : BackgroundService
    {
        private readonly IHubContext<NewObjectsHub> _hubContext;
        private readonly int _port = 9000;
        private UdpClient _udpSender;
        private UdpClient _udpListener;
        private readonly Random _rnd = new Random();

        public UdpForwarderService(IHubContext<NewObjectsHub> hubContext)
        {
            _hubContext = hubContext;
            // Инициализация UdpClient для отправки (без привязки) и для приема (на том же порту)
            _udpSender = new UdpClient(); 
            _udpListener = new UdpClient(_port);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Запускаем параллельно две задачи: отправку и прием UDP данных.
            var sendingTask = SendUdpDataAsync(stoppingToken);
            var receivingTask = ReceiveUdpDataAsync(stoppingToken);

            await Task.WhenAll(sendingTask, receivingTask);
        }

        private async Task SendUdpDataAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                // Формирование реальных данных телескопа
                string telemetryData = $"Телескоп: ID-{Guid.NewGuid()}, координаты: ({_rnd.Next(0, 360)},{_rnd.Next(0, 90)})";
                byte[] data = Encoding.UTF8.GetBytes(telemetryData);
                
                // Отправляем данные на локальный порт (127.0.0.1:9000)
                await _udpSender.SendAsync(data, data.Length, "127.0.0.1", _port);
                Console.WriteLine("Отправлены UDP данные: " + telemetryData);
                await Task.Delay(5000, stoppingToken); // интервал 5 секунд
            }
        }

        private async Task ReceiveUdpDataAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var result = await _udpListener.ReceiveAsync();
                    string receivedData = Encoding.UTF8.GetString(result.Buffer);
                    Console.WriteLine("Получены UDP данные: " + receivedData);
                    // Пересылаем полученные данные через SignalR
                    await _hubContext.Clients.All.SendAsync("ReceiveUdpData", receivedData);
                }
                catch (ObjectDisposedException)
                {
                    // Если UdpListener закрыт
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Ошибка UDP слушателя: " + ex.Message);
                }
            }
        }

        public override void Dispose()
        {
            _udpSender?.Dispose();
            _udpListener?.Dispose();
            base.Dispose();
        }
    }