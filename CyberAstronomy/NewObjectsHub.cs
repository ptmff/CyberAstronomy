using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace CyberAstronomy;

public class NewObjectsHub : Hub
{
    public override Task OnConnectedAsync()
    {
        System.Console.WriteLine($"Клиент с ID {Context.ConnectionId} подключен к SignalR.");
        return base.OnConnectedAsync();
    }
}