using System.Diagnostics;
using BDSManager.WebUI.Models;
using BDSManager.WebUI.Hubs;
using BDSManager.WebUI.IO;
using Newtonsoft.Json;

namespace BDSManager.WebUI.Services;

public class MinecraftServerService
{
    public readonly List<ServerInstance> ServerInstances;
    private readonly string? _serversPath;
    private readonly IConfiguration _configuration;
    private readonly ConsoleHub _consoleHub;
    private readonly OptionsIO _optionsIO;
    private readonly ServerProperties _serverProperties;
    private readonly IHostApplicationLifetime _appLifetime;
    private readonly int MAX_LOG_LINES = 1000;
    private readonly int MAX_PROCESS_TIMEOUT = 10000;

    public MinecraftServerService(IConfiguration configuration, ConsoleHub consoleHub, OptionsIO optionsIO, ServerProperties serverProperties, IHostApplicationLifetime appLifetime)
    {
        _configuration = configuration;
        _consoleHub = consoleHub;
        _optionsIO = optionsIO;
        _serverProperties = serverProperties;
        _appLifetime = appLifetime;
        ServerInstances = new();
        _serversPath = _configuration["ServersPath"];
        RestartExistingProcesses();
    }

    private void RestartExistingProcesses()
    {
        var processes = Process.GetProcessesByName("bedrock_server");
        processes.Where(x => !string.IsNullOrEmpty(x.MainModule?.FileName)).ToList().ForEach(x =>
        {
            x.Kill();
            var serverPath = Path.GetFileName(Path.GetDirectoryName(x.MainModule?.FileName));
            var server = _optionsIO.ManagerOptions.Servers.FirstOrDefault(y => y.Path == serverPath);
            if (server == null)
                return;
            StartServerInstance(server);
        });

        _appLifetime.ApplicationStopping.Register(() =>
        {
            ServerInstances.ForEach(x => StopServerInstance(x));
        });
    }

    public ServerInstance CreateServerInstance(ServerModel server)
    {
        if (string.IsNullOrEmpty(server.Path))
            throw new Exception("Server path is not set");
        
        if (string.IsNullOrEmpty(_serversPath))
            throw new Exception("ServersPath is not set in appsettings.json");

        var serverPath = Path.Combine(_serversPath, server.Path);
        var serverInstance = new ServerInstance
        {
            Path = server.Path,
            ServerProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = Path.Combine(serverPath,"bedrock_server.exe"),
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            }
        };
        
        serverInstance.ServerProcess.OutputDataReceived += (sender, e) => ServerProcess_OutputDataReceived(sender, e, serverInstance);
        
        ServerInstances.Add(serverInstance);
        return serverInstance;
    }

    public void StartServerInstance(ServerModel server)
    {
        var instance = ServerInstances.FirstOrDefault(x => x.Path == server.Path);
        if(instance == null)
            instance = CreateServerInstance(server);
        
        if(string.IsNullOrEmpty(instance.Path))
        {
            _consoleHub.UpdateConsoleOutput("-1", "CONTROL:start-failed");
            return;
        }
        instance.ConsoleOutput.Clear();
        if(instance.ServerProcess == null)
        {
            _consoleHub.UpdateConsoleOutput(instance.Path, "CONTROL:start-failed");
            return;
        }
        instance.ServerProcess.Start();
        instance.ServerProcess.BeginOutputReadLine();

        // check if server is running
        if (instance.ServerProcess.HasExited)
            _consoleHub.UpdateConsoleOutput(instance.Path, "CONTROL:start-failed");
        else
            _consoleHub.UpdateConsoleOutput(instance.Path, "CONTROL:start-success");
    }

    public void StopServerInstance(ServerInstance instance)
    {
        if(string.IsNullOrEmpty(instance.Path))
        {
            _consoleHub.UpdateConsoleOutput("-1", "CONTROL:stop-failed");
            return;
        }
        if(instance.ServerProcess == null)
        {
            _consoleHub.UpdateConsoleOutput(instance.Path, "CONTROL:stop-already-stopped");
            instance.ConsoleOutput.Clear();
            ServerInstances.Remove(instance);
            return;
        }

        if (!instance.ServerProcess.HasExited)
        {
            StopServerProcess(instance);

            instance.ConsoleOutput.Clear();
            ServerInstances.Remove(instance);
        }
    }

    public void RestartServerInstance(ServerInstance instance)
    {
        if(string.IsNullOrEmpty(instance.Path))
        {
            _consoleHub.UpdateConsoleOutput("-1", "CONTROL:stop-failed");
            return;
        }
        if(instance.ServerProcess == null)
        {
            _consoleHub.UpdateConsoleOutput(instance.Path, "CONTROL:stop-already-stopped");
            instance.ConsoleOutput.Clear();
            ServerInstances.Remove(instance);
            return;
        }

        if (!instance.ServerProcess.HasExited)
        {
            StopServerProcess(instance);
            instance.ConsoleOutput.Clear();
        }
        instance.ServerProcess.Start();
    }

    public async Task<string> SendCommandToServerInstance(string serverPath, string command)
    {
        var serverInstance = ServerInstances.FirstOrDefault(x => x.Path == serverPath);
        if(serverInstance == null)
            return "Server not running";
        return await SendCommandToServerInstance(serverInstance, command);
    }

    private Task<string> SendCommandToServerInstance(ServerInstance instance, string command)
    {
        if(instance.ServerProcess == null || instance.ServerProcess.HasExited)
            return Task.FromResult("Server not running");

        instance.ServerProcess.StandardInput.WriteLine(command);
        return Task.FromResult("sent");
    }

    private void ServerProcess_OutputDataReceived(object sender, DataReceivedEventArgs e, ServerInstance instance)
    {
        if(string.IsNullOrEmpty(e.Data) || string.IsNullOrWhiteSpace(instance.Path))
            return;
        
        ProcessConsoleOutput(instance, e.Data);

        instance.ConsoleOutput.AddLast(e.Data);
        _consoleHub.UpdateConsoleOutput(instance.Path, e.Data);

        if (instance.ConsoleOutput.Count > MAX_LOG_LINES)
            instance.ConsoleOutput.RemoveFirst();
    }

    private void StopServerProcess(ServerInstance instance)
    {
    if(instance.ServerProcess == null || string.IsNullOrEmpty(instance.Path))
            return;
        
        if (!instance.ServerProcess.HasExited)
        {
            try
            {
                instance.ServerProcess.StandardInput.WriteLine("stop");
            }
            catch (InvalidOperationException)
            {
                instance.ServerProcess.Kill();
            }
            instance.ServerProcess.WaitForExit(MAX_PROCESS_TIMEOUT);

            if (instance.ServerProcess.HasExited)
                _consoleHub.UpdateConsoleOutput(instance.Path, "CONTROL:stop-success");
            else
            {
                _consoleHub.UpdateConsoleOutput(instance.Path, "CONTROL:stop-failed");
                instance.ServerProcess.Kill();
                instance.ServerProcess.WaitForExit();
                _consoleHub.UpdateConsoleOutput(instance.Path, "CONTROL:stop-success");
            }
        }
    }

    private void ProcessConsoleOutput(ServerInstance instance, string output)
    {
        if (string.IsNullOrEmpty(output) || string.IsNullOrWhiteSpace(instance.Path))
            return;

        // add or remove player on connect/disconnect
        var playerConnected = output.Contains("Player connected:");
        var playerDisconnected = output.Contains("Player disconnected:");
        if (playerConnected || playerDisconnected)
        {
            ProcessPlayer(instance, output, playerConnected);
            return;
        }
    }

    private void ProcessPlayer(ServerInstance instance, string output, bool playerConnected){
        var logParts = playerConnected ? output.Split("Player connected:")[1].Trim().Split(',') : output.Split("Player disconnected:")[1].Trim().Split(',');
        var playerName = logParts[0].Trim();
        var xuid = logParts[1].Split("xuid:")[1].Trim();
        var server = _optionsIO.ManagerOptions.Servers.FirstOrDefault(x => x.Path == instance.Path);
        if(server == null || string.IsNullOrEmpty(instance.Path))
            return;
        var player = new PlayerModel
        {
            Name = playerName,
            XUID = xuid,
            Online = playerConnected,
            LastSeen = DateTime.Now
        };
        
        if(server.Players.Any(x => x.XUID == xuid))
        {
            var playerToUpdate = server.Players.FirstOrDefault(x => x.XUID == xuid) ?? player;
            playerToUpdate.Online = playerConnected;
            playerToUpdate.LastSeen = DateTime.Now;
        }
        else
        {
            server.Players.Add(player);
        }
        _consoleHub.UpdateConsoleOutput(instance.Path, $"CONTROL:player-count-update|{server.Players.Where(x => x.Online).Count()}/{server.Options.MaxPlayers}");
        _consoleHub.UpdateConsoleOutput(instance.Path, $"CONTROL:player-list-update|{JsonConvert.SerializeObject(server.Players)}");
        _serverProperties.SavePlayers(server);
    }
}