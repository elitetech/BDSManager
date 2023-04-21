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
    private readonly string? _backupsPath;
    private readonly IConfiguration _configuration;
    private readonly ConsoleHub _consoleHub;
    private readonly OptionsIO _optionsIO;
    private readonly ServerProperties _serverProperties;
    private readonly IHostApplicationLifetime _appLifetime;
    private readonly BDSBackup _bdsBackup;
    private readonly int MAX_LOG_LINES = 1000;
    private readonly int MAX_PROCESS_TIMEOUT = 10000;
    private const int SHUTDOWN_WARNING_TIME = 1000 * 60;

    public MinecraftServerService(IConfiguration configuration, ConsoleHub consoleHub, OptionsIO optionsIO, ServerProperties serverProperties, BDSBackup bdsBackup, IHostApplicationLifetime appLifetime)
    {
        _configuration = configuration;
        _consoleHub = consoleHub;
        _optionsIO = optionsIO;
        _serverProperties = serverProperties;
        _bdsBackup = bdsBackup;
        _appLifetime = appLifetime;
        ServerInstances = new();
        _serversPath = _configuration["ServersPath"];
        _backupsPath = _configuration["BackupsPath"];
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
            StartServerInstance(server).GetAwaiter();
        });

        _appLifetime.ApplicationStopping.Register(() =>
        {
            ServerInstances.ForEach(x => StopServerInstance(x).GetAwaiter());
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

    public Task StartServerInstance(ServerModel server)
    {
        var instance = ServerInstances.FirstOrDefault(x => x.Path == server.Path);
        if(instance == null)
            instance = CreateServerInstance(server);
        
        if(string.IsNullOrEmpty(instance.Path))
        {
            _consoleHub.UpdateConsoleOutput("-1", "CONTROL:start-failed");
            return Task.CompletedTask;
        }
        instance.ConsoleOutput.Clear();
        if(instance.ServerProcess == null)
        {
            _consoleHub.UpdateConsoleOutput(instance.Path, "CONTROL:start-failed");
            return Task.CompletedTask;
        }
        instance.ServerProcess.Start();
        instance.ServerProcess.BeginOutputReadLine();

        // check if server is running
        if (instance.ServerProcess.HasExited)
            _consoleHub.UpdateConsoleOutput(instance.Path, "CONTROL:start-failed");
        else
            _consoleHub.UpdateConsoleOutput(instance.Path, "CONTROL:start-success");

        return Task.CompletedTask;
    }

    public async Task StopServerInstance(ServerInstance instance)
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
            await StopServerProcess(instance);

            instance.ConsoleOutput.Clear();
            ServerInstances.Remove(instance);
        }
    }

    public async Task RestartServerInstance(ServerInstance instance)
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
            await StopServerProcess(instance);
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
        if(string.IsNullOrEmpty(e.Data) || string.IsNullOrWhiteSpace(instance.Path) || e.Data.Contains("AutoCompaction"))
            return;
        
        ProcessConsoleOutput(instance, e.Data);

        instance.ConsoleOutput.AddLast(e.Data);
        _consoleHub.UpdateConsoleOutput(instance.Path, e.Data);

        if (instance.ConsoleOutput.Count > MAX_LOG_LINES)
            instance.ConsoleOutput.RemoveFirst();
    }

    internal async Task BackupServer(ServerModel server)
    {
        if(string.IsNullOrEmpty(_backupsPath))
            return;
        if(!Directory.Exists(_backupsPath))
            Directory.CreateDirectory(_backupsPath);
        
        var destinationPath = Path.Combine(_backupsPath, server.Path);
        if(!Directory.Exists(destinationPath))
            Directory.CreateDirectory(destinationPath);
        
        var instance = ServerInstances.FirstOrDefault(x => x.Path == server.Path);
        var running = instance != null && instance.ServerProcess != null && !instance.ServerProcess.HasExited;

        if (running && instance != null)
            await StopServerInstance(instance);
        
        await _bdsBackup.Backup(server);

        if(running)
            await StartServerInstance(server);
        return;
    }

    private async Task StopServerProcess(ServerInstance instance)
    {
        if(instance.ServerProcess == null || string.IsNullOrEmpty(instance.Path))
            return;
        
        if (!instance.ServerProcess.HasExited)
        {
            await WarnPlayerOfShutdown(instance);
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
        return;
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

    private Task WarnPlayerOfShutdown(ServerInstance instance, int timeLeft = SHUTDOWN_WARNING_TIME)
    {
        if(instance.ServerProcess == null || instance.ServerProcess.HasExited)
            return Task.CompletedTask;

        while(instance.ServerProcess != null && !instance.ServerProcess.HasExited)
        {
            if(timeLeft <= 0)
                break;
            var nextWarning = SendShutdownWarning(instance, timeLeft);
            if(nextWarning == 0)
                break;
            timeLeft -= nextWarning;
            Thread.Sleep(nextWarning);
        }
        return Task.CompletedTask;
    }

    private int SendShutdownWarning(ServerInstance instance, int timeLeft)
    {
        var nextWarning = 0;
        if(instance.ServerProcess == null || instance.ServerProcess.HasExited || timeLeft <= 0)
            return nextWarning;

        switch (timeLeft)
        {
            case 60000:
                nextWarning = 30000;
                break;
            case 30000:
                nextWarning = 20000;
                break;
            case 10000:
                nextWarning = 5000;
                break;
            case 5000:
                nextWarning = 2000;
                break;
            case 3000:
                nextWarning = 1000;
                break;
            case 2000:
                nextWarning = 1000;
                break;
            case 1000:
                nextWarning = 1000;
                break;
            default:
                return nextWarning;
        }
        var plural = timeLeft / 1000 == 1 ? "" : "s";
        instance.ServerProcess?.StandardInput.WriteLine($"say Server shutting down in {timeLeft / 1000} second{plural}");

        return nextWarning;
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