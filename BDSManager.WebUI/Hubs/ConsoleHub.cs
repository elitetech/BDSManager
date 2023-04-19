using Microsoft.AspNetCore.SignalR;

namespace BDSManager.WebUI.Hubs;

public class ConsoleHub : Hub
{
    public void UpdateConsoleOutput(string server, string output)
    {
        try
        {
            Clients.All.SendAsync("updateConsoleOutput", server, output);
        }
        catch
        {
            // ignored
        }
    }
}