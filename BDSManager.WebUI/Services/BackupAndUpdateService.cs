
using BDSManager.WebUI.IO;
using BDSManager.WebUI.Models;

namespace BDSManager.WebUI.Services;

public class BackupAndUpdateService : IHostedService, IDisposable
{
    private readonly OptionsIO _optionsIO;
    private readonly BDSBackup _bdsBackup;
    private readonly BDSUpdater _bdsUpdater;
    private readonly ServerProperties _serverProperties;
    private Timer? _timer;

    public BackupAndUpdateService(OptionsIO optionsIO, BDSBackup bdsBackup, BDSUpdater bdsUpdater, ServerProperties serverProperties)
    {
        _optionsIO = optionsIO;
        _bdsBackup = bdsBackup;
        _bdsUpdater = bdsUpdater;
        _serverProperties = serverProperties;
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
        RunScheduledBackups(state);
        RunScheduledUpdates(state);
    }

    private void RunScheduledBackups(object? state)
    {
        foreach (var server in _optionsIO.ManagerOptions.Servers)
        {
            if (!server.Backup.BackupEnabled)
                continue;
            
            if (server.Backup.NextBackup == null || server.Backup.NextBackup <= DateTime.Now)
            {
                _bdsBackup.Backup(server);
                server.Backup.NextBackup = DateTime.Now.AddHours(server.Backup.BackupInterval);
                _serverProperties.SaveBackupSettings(server);
            }
        }
    }

    private async void RunScheduledUpdates(object? state)
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