using CyberAstronomy;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Добавляем поддержку SignalR
builder.Services.AddSignalR();

// Регистрируем фоновые сервисы
builder.Services.AddHostedService<TCPServerBackgroundService>();
builder.Services.AddHostedService<NewObjectGeneratorBackgroundService>();
builder.Services.AddHostedService<EventNotifierBackgroundService>();
builder.Services.AddHostedService<UdpDataServerBackgroundService>();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

// Маршрут для SignalR-хаба
app.MapHub<NewObjectsHub>("/newObjectHub");

app.Run();