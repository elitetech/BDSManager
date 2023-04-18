using BDSManager.WebUI.IO;
using BDSManager.WebUI.Services;
using Microsoft.AspNetCore.SignalR;

namespace BDSManager.WebUI.Hubs;

public class CommandHub : Hub   
{
    private readonly MinecraftServerService _minecraftServerService;
    private readonly OptionsIO _optionsIO;
    private readonly ConsoleHub _consoleHub;

    public CommandHub(MinecraftServerService minecraftServerService, OptionsIO optionsIO, ConsoleHub consoleHub)
    {
        _minecraftServerService = minecraftServerService;
        _optionsIO = optionsIO;
        _consoleHub = consoleHub;
    }
    public async Task SendCommand(string path, string command)
    {
        if(string.IsNullOrEmpty(path) || string.IsNullOrEmpty(command))
            return;

        if(command == "stop" || command == "restart" || command == "start")
        {
            var instance = _minecraftServerService.ServerInstances.FirstOrDefault(x => x.Path == path);
            var server = _optionsIO.ManagerOptions.Servers.FirstOrDefault(x => x.Path == path);
            if(server == null && instance == null){
                _consoleHub.UpdateConsoleOutput(path, "Server not found");
                return;
            }
            if(command == "stop")
                if(instance == null)
                    _consoleHub.UpdateConsoleOutput(path, "Server not found");
                else
                    _minecraftServerService.StopServerInstance(instance);
            else if(command == "restart")
                if(instance == null)
                    _consoleHub.UpdateConsoleOutput(path, "Server not found");
                else
                    _minecraftServerService.RestartServerInstance(instance);
            else if(command == "start")
                if(server == null)
                    _consoleHub.UpdateConsoleOutput(path, "Server not found");
                else
                    _minecraftServerService.StartServerInstance(server);
            return;
        }
        var result = await _minecraftServerService.SendCommandToServerInstance(path, command);
        if(result != "sent")
            _consoleHub.UpdateConsoleOutput(path, string.IsNullOrEmpty(result) ? "Command not sent" : result);
    }
}