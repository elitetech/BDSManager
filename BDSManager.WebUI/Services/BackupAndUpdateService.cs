
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
    }

    private async void RunScheduledBackups()
    {
        foreach (var server in _optionsIO.ManagerOptions.Servers)
        {
            if (!server.Backup.BackupEnabled)
                continue;
            
            if (server.Backup.NextBackup == null || server.Backup.NextBackup <= DateTime.Now)
            {
                
                var instance = _minecraftServerService.ServerInstances.Any(x => x.Path == server.Path) ? _minecraftServerService.ServerInstances.FirstOrDefault(x => x.Path == server.Path) : null;
                if(instance != null && instance.ServerProcess != null && !instance.ServerProcess.HasExited)
                    await _minecraftServerService.StopServerInstance(instance);

                await _bdsBackup.Backup(server);
                server.Backup.NextBackup = DateTime.Now.AddHours(server.Backup.BackupInterval);
                _serverProperties.SaveBackupSettings(server);
            }
        }
    }

    private async void RunScheduledUpdates()
    {
        foreach (var server in _optionsIO.ManagerOptions.Servers)
        {
            if (!server.Update.UpdateEnabled)
                continue;
            
            if (server.Update.NextUpdate == null || server.Update.NextUpdate <= DateTime.Now)
            {
                await _bdsUpdater.UpdateBedrockServerAsync(server);
                server.Update.NextUpdate = DateTime.Now.AddHours(server.Update.UpdateInterval);
                _serverProperties.SaveUpdateSettings(server);
            }
        }
    }
}