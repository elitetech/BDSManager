using Microsoft.AspNetCore.SignalR;

namespace BDSManager.WebUI.Hubs;

public class ConsoleHub : Hub
{
    public void UpdateConsoleOutput(string server, string output)
    {
        Clients.All.SendAsync("updateConsoleOutput", server, output);
    }
}