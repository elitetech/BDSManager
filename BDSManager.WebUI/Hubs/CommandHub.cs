using BDSManager.WebUI.Services;
using Microsoft.AspNetCore.SignalR;

namespace BDSManager.WebUI.Hubs;

public class CommandHub : Hub   
{
    private readonly MinecraftServerService _minecraftServerService;
    private readonly ConsoleHub _consoleHub;

    public CommandHub(MinecraftServerService minecraftServerService, ConsoleHub consoleHub)
    {
        _minecraftServerService = minecraftServerService;
        _consoleHub = consoleHub;
    }
    public async Task SendCommand(string server, string command)
    {
        var result = await _minecraftServerService.SendCommandToServerInstance(server, command);
        if(result != "sent")
            _consoleHub.UpdateConsoleOutput(server, string.IsNullOrEmpty(result) ? "Command not sent" : result);
    }
}