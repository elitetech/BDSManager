using Microsoft.AspNetCore.SignalR;

namespace BDSManager.WebUI.Hubs;

public class MinecraftServerHub : Hub
{
    public void UpdateConsoleOutput(string server, string command)
    {
        Clients.All.SendAsync("updateConsoleOutput", server, command);
    }
}