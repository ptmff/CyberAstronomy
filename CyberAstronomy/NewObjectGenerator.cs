using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CyberAstronomy;

public class NewObjectGenerator
{
    private readonly IHubContext<NewObjectsHub> _hubContext;
    private readonly Random _rnd = new Random();

    public NewObjectGenerator(IHubContext<NewObjectsHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        // Генерировать новые объекты с реальным интервалом от 1 до 5 минут.
        while (!cancellationToken.IsCancellationRequested)
        {
            string newObjectData = $"Новый объект в космосе: ID-{Guid.NewGuid()}, координаты: ({_rnd.Next(0, 1000)},{_rnd.Next(0, 1000)})";
            await _hubContext.Clients.All.SendAsync("ReceiveNewObject", newObjectData);
            Console.WriteLine("Отправлен новый объект: " + newObjectData);
                
            int delay = _rnd.Next(60000, 300000);
            await Task.Delay(delay, cancellationToken);
        }
    }
}