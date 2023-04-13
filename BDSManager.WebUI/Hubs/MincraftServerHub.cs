using Microsoft.AspNetCore.SignalR;

namespace BDSManager.WebUI.Hubs;

public class MinecraftServerHub : Hub
{
    public void SendCommand(string command)
    {
        // Call the broadcastMessage method to update clients.
        Clients.All.SendAsync("broadcastMessage", command);
    }
}