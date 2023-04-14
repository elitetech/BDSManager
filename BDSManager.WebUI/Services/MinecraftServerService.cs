using System.Diagnostics;
using BDSManager.WebUI.Models;
using BDSManager.WebUI.Hubs;

namespace BDSManager.WebUI.Services;

public class MinecraftServerService
{
    private readonly List<ServerInstance> _serverInstances;
    private readonly string? _serversPath;
    private readonly IConfiguration _configuration;
    private readonly ConsoleHub _consoleHub;
    private readonly int _maxConsoleCacheLines = 100;

    public MinecraftServerService(IConfiguration configuration, ConsoleHub consoleHub)
    {
        _configuration = configuration;
        _consoleHub = consoleHub;
        _serverInstances = new();
        _serversPath = _configuration["ServersPath"];
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
                    FileName = serverPath,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            }
        };
        
        serverInstance.ServerProcess.OutputDataReceived += (sender, e) =>
        {
            ServerProcess_OutputDataReceived(sender, e, serverInstance);
        };
        _serverInstances.Add(serverInstance);
        return serverInstance;
    }

    public void StartServerInstance(ServerInstance instance)
    {
        if(instance.ServerProcess == null)
            throw new Exception("Server not running");

        instance.ServerProcess.Start();
        instance.ServerProcess.BeginOutputReadLine();
    }

    public void StopServerInstance(ServerInstance instance)
    {
        if(instance.ServerProcess == null)
            throw new Exception("Server not running");

        if (!instance.ServerProcess.HasExited)
        {
            instance.ServerProcess.StandardInput.WriteLine("stop");
            instance.ServerProcess.WaitForExit();
        }
    }

    public async Task<string> SendCommandToServerInstance(string serverPath, string command)
    {
        var serverInstance = _serverInstances.FirstOrDefault(x => x.Path == serverPath);
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
        
        instance.ConsoleOutput.AddLast(e.Data);
        _consoleHub.UpdateConsoleOutput(instance.Path, e.Data);

        if (instance.ConsoleOutput.Count > _maxConsoleCacheLines)
            instance.ConsoleOutput.RemoveFirst();
    }
}