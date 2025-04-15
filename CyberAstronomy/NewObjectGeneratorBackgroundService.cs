using Microsoft.AspNetCore.SignalR;

namespace CyberAstronomy;

public class NewObjectGeneratorBackgroundService : BackgroundService
{
    private readonly NewObjectGenerator _generator;

    public NewObjectGeneratorBackgroundService(IHubContext<NewObjectsHub> hubContext)
    {
        _generator = new NewObjectGenerator(hubContext);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _generator.StartAsync(stoppingToken);
    }
}