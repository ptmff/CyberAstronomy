using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace CyberAstronomy;

public class EventNotifier
{
    private readonly IHubContext<NewObjectsHub> _hubContext;

    public EventNotifier(IHubContext<NewObjectsHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task NotifyEventAsync()
    {
        string eventMessage = $"Событие дня: уникальный артефакт найден! Время: {DateTime.Now}";
        await _hubContext.Clients.All.SendAsync("ReceiveEvent", eventMessage);
        Console.WriteLine("Уведомление отправлено: " + eventMessage);
    }
}