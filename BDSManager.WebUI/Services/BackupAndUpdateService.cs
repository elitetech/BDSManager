
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
    private Timer? _timer;

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

    private void RunJobs(object? state)
    {
        RunScheduledBackups();
        RunScheduledUpdates();
        if (!_ranOnce)
            AutoStartServers();
        _ranOnce = true;
        
        _optionsIO.RefreshServers();
    }

    private async void AutoStartServers()
    {
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

    private async void RunScheduledBackups()
    {
        var serversCount = _optionsIO.ManagerOptions.Servers.Where(x => x.Backup.BackupEnabled).Count();
        if (serversCount == 0)
            return;
        for(var i = 0; i < serversCount; i++)
        {
            var server = _optionsIO.ManagerOptions.Servers.Where(x => x.Backup.BackupEnabled).ElementAt(i);
            if(server.LastStarted == null)
                continue;
            
            if(server.Backup.NextBackup != null && server.Backup.NextBackup > DateTime.Now)
                continue;

            await _minecraftServerService.BackupServer(server);
            server.Backup.NextBackup = DateTime.Now.AddHours(server.Backup.BackupInterval);
            _serverProperties.SaveBackupSettings(server);
        }
    }

    private async void RunScheduledUpdates()
    {
        var serversCount = _optionsIO.ManagerOptions.Servers.Where(x => x.Update.UpdateEnabled).Count();
        if (serversCount == 0)
            return;
        for(var i = 0; i < serversCount; i++)
        {
            var server = _optionsIO.ManagerOptions.Servers.Where(x => x.Update.UpdateEnabled).ElementAt(i);
            if(server.Update.NextUpdate != null && server.Update.NextUpdate > DateTime.Now)
                continue;
            await _bdsUpdater.UpdateBedrockServerAsync(server);
            server.Update.NextUpdate = DateTime.Now.AddHours(server.Update.UpdateInterval);
            _serverProperties.SaveUpdateSettings(server);
        }
    }
}