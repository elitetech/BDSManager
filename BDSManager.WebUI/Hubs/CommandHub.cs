using BDSManager.WebUI.IO;
using BDSManager.WebUI.Services;
using Microsoft.AspNetCore.SignalR;

namespace BDSManager.WebUI.Hubs;

public class CommandHub : Hub   
{
    private readonly MinecraftServerService _minecraftServerService;
    private readonly OptionsIO _optionsIO;
    private readonly ServerProperties _serverProperties;
    private readonly ConsoleHub _consoleHub;
    private readonly BDSUpdater _bdsUpdater;
    private readonly string[] _controlCommands = new string[] {"stop", "restart", "start", "backup", "update"};

    public CommandHub(MinecraftServerService minecraftServerService, OptionsIO optionsIO, ConsoleHub consoleHub, ServerProperties serverProperties, BDSUpdater bdsUpdater)
    {
        _minecraftServerService = minecraftServerService;
        _optionsIO = optionsIO;
        _consoleHub = consoleHub;
        _serverProperties = serverProperties;
        _bdsUpdater = bdsUpdater;
    }
    public async Task SendCommand(string path, string command)
    {
        if(string.IsNullOrEmpty(path) || string.IsNullOrEmpty(command))
            return;

        if(command == "stop" || command == "restart" || command == "start" || command == "backup" || command == "update")
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
                    await _minecraftServerService.StopServerInstance(instance, "STOP");
            else if(command == "restart")
                if(instance == null)
                    _consoleHub.UpdateConsoleOutput(path, "Server not found");
                else
                    await _minecraftServerService.RestartServerInstance(instance);
            else if(command == "start")
                if(server == null)
                    _consoleHub.UpdateConsoleOutput(path, "Server not found");
                else
                    await _minecraftServerService.StartServerInstance(server);
            else if(command == "backup")
            {
                if(server == null)
                    _consoleHub.UpdateConsoleOutput(path, "Server not found");
                else
                    await _minecraftServerService.BackupServer(server);
            }
            else if(command == "update")
            {
                if(server == null)
                    _consoleHub.UpdateConsoleOutput(path, "Server not found");
                else
                    await _bdsUpdater.UpdateBedrockServerAsync(server);
            }
            return;
        }

        

        var result = await _minecraftServerService.SendCommandToServerInstance(path, command);
        if(result != "sent")
            _consoleHub.UpdateConsoleOutput(path, string.IsNullOrEmpty(result) ? "Command not sent" : result);

        if(command.Contains("whitelist"))
        {
            _optionsIO.RefreshServers();
        }
    }
}