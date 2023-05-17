
using BDSManager.WebUI.IO;
using BDSManager.WebUI.Models;

namespace BDSManager.WebUI.Services;

public class BackupAndUpdateService : IHostedService, IDisposable
{
    private readonly OptionsIO _optionsIO;
    private readonly BDSBackup _bdsBackup;
    private readonly BDSUpdater _bdsUpdater;
    private readonly ServerProperties _serverProperties;
    private readonly MinecraftServerService _minecraftServerService;
    private bool _ranOnce = false;
    private bool _backingUp = false;
    private bool _backingWorldUp = false;
    private bool _updating = false;
    private Timer? _timer;
    private const int TASK_DELAY = 1000 * 10;

    public BackupAndUpdateService(OptionsIO optionsIO, BDSBackup bdsBackup, BDSUpdater bdsUpdater, ServerProperties serverProperties, MinecraftServerService minecraftServerService)
    {
        _optionsIO = optionsIO;
        _bdsBackup = bdsBackup;
        _bdsUpdater = bdsUpdater;
        _serverProperties = serverProperties;
        _minecraftServerService = minecraftServerService;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _timer = new Timer(RunJobs, null, TimeSpan.Zero, TimeSpan.FromMinutes(5));
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    public void Dispose() => _timer?.Dispose();

    private async void RunJobs(object? state)
    {
        RunScheduledBackups();
        RunScheduledUpdates();
        AutoStartServers();
        await Task.Delay(TASK_DELAY);
        RunScheduledWorldBackups();
        
        _optionsIO.RefreshServers();
    }

    private async void AutoStartServers()
    {
        if(_ranOnce || _backingUp || _updating)
            return;
        _ranOnce = true;
        var serversCount = _optionsIO.ManagerOptions.Servers.Where(x => x.AutoStartEnabled).Count();
        if (serversCount == 0)
            return;
        for(var i = 0; i < serversCount; i++)
        {
            var server = _optionsIO.ManagerOptions.Servers.Where(x => x.AutoStartEnabled).ElementAt(i);
            var instance = _minecraftServerService.ServerInstances.Any(x => x.Path == server.Path) ? _minecraftServerService.ServerInstances.FirstOrDefault(x => x.Path == server.Path) : null;
            if (instance == null || instance.ServerProcess == null || instance.ServerProcess.HasExited)
                await _minecraftServerService.StartServerInstance(server);
        }
    }

    private async void RunScheduledWorldBackups()
    {
        _backingWorldUp = true;
        var serversCount = _optionsIO.ManagerOptions.Servers.Where(x => x.Backup.WorldBackupEnabled).Count();
        if (serversCount == 0)
            return;
        var didBackup = false;
        for(var i = 0; i < serversCount; i++)
        {
            var server = _optionsIO.ManagerOptions.Servers.Where(x => x.Backup.WorldBackupEnabled).ElementAt(i);
            var instance = _minecraftServerService.ServerInstances.FirstOrDefault(x => x.Path == server.Path);
            if(instance == null || string.IsNullOrEmpty(instance.Path) || instance.ServerProcess == null || instance.ServerProcess.HasExited)
                continue;

            await _minecraftServerService.SendCommandToServerInstance(instance.Path, "save hold");
            didBackup = true;
        }
        if (didBackup) 
            _optionsIO.RefreshServers();
        _backingWorldUp = false;
    }

    private async void RunScheduledBackups()
    {
        _backingUp = true;
        var serversCount = _optionsIO.ManagerOptions.Servers.Where(x => x.Backup.BackupEnabled).Count();
        if (serversCount == 0)
            return;
        var didBackup = false;
        for(var i = 0; i < serversCount; i++)
        {
            var server = _optionsIO.ManagerOptions.Servers.Where(x => x.Backup.BackupEnabled).ElementAt(i);
            if(server.LastStarted == null)
                continue;
            
            if(server.Backup.NextBackup != null && server.Backup.NextBackup > DateTime.Now)
                continue;

            await _minecraftServerService.BackupServer(server);
            server.Backup.NextBackup = DateTime.Now.AddHours(server.Backup.BackupInterval);
            _serverProperties.SaveServerSettings(server);
            didBackup = true;
        }
        if (didBackup) 
            _optionsIO.RefreshServers();
        _backingUp = false;
    }

    private async void RunScheduledUpdates()
    {
        _updating = true;
        var serversCount = _optionsIO.ManagerOptions.Servers.Where(x => x.Update.UpdateEnabled).Count();
        if (serversCount == 0)
            return;
        var didUpdate = false;
        for(var i = 0; i < serversCount; i++)
        {
            var server = _optionsIO.ManagerOptions.Servers.Where(x => x.Update.UpdateEnabled).ElementAt(i);
            if(server.Update.NextUpdate != null && server.Update.NextUpdate > DateTime.Now)
                continue;
            await _bdsUpdater.UpdateBedrockServerAsync(server);
            server.Update.NextUpdate = DateTime.Now.AddHours(server.Update.UpdateInterval);
            _serverProperties.SaveServerSettings(server);
            didUpdate = true;
        }
        if (didUpdate) 
            _optionsIO.RefreshServers();
        _updating = false;
    }

    public async Task SaveWorld(ServerModel server)
    {
        var instance = _minecraftServerService.ServerInstances.FirstOrDefault(x => x.Path == server.Path);
        if(instance == null || string.IsNullOrEmpty(instance.Path) || instance.ServerProcess == null || instance.ServerProcess.HasExited)
            return;
        await _minecraftServerService.SendCommandToServerInstance(instance.Path, "save hold");
        return;
    }
}