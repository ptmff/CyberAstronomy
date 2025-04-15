using Microsoft.AspNetCore.SignalR;

namespace CyberAstronomy;

public class EventNotifierBackgroundService : BackgroundService
{
    private readonly EventNotifier _notifier;

    public EventNotifierBackgroundService(IHubContext<NewObjectsHub> hubContext)
    {
        _notifier = new EventNotifier(hubContext);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Каждые 5 минут отсылаем событие дня
        while (!stoppingToken.IsCancellationRequested)
        {
            await _notifier.NotifyEventAsync();
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}