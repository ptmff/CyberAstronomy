using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace CyberAstronomy;

public class UdpDataServer
{
    private readonly UdpClient _udpClient;
    private readonly IPEndPoint _localEp;

    public UdpDataServer(string ipAddress, int port)
    {
        _localEp = new IPEndPoint(IPAddress.Parse(ipAddress), port);
        _udpClient = new UdpClient(_localEp);
    }

    // Метод для отправки данных телескопа (в примере отправка данных по UDP каждые 5 секунд)
    public async Task StartSendingDataAsync()
    {
        while (true)
        {
            // Генерация данных – координаты, интенсивность сигнала, и пр.
            string telemetryData = $"Телескоп: ID-{Guid.NewGuid()}, координаты: ({new Random().Next(0, 360)},{new Random().Next(0, 90)})";
            byte[] data = Encoding.UTF8.GetBytes(telemetryData);
            // Отправляем данные на заранее известный адрес клиента (либо можно реализовать мультикаст)
            // В данном примере данные отправляются на localhost, порт 9000 (измените под нужды)
            await _udpClient.SendAsync(data, data.Length, "127.0.0.1", 9000);
            Console.WriteLine("Отправлены UDP данные: " + telemetryData);
            await Task.Delay(5000); // интервал 5 сек
        }
    }
}