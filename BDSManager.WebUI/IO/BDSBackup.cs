using System.IO.Compression;
using BDSManager.WebUI.Models;

namespace BDSManager.WebUI.IO;

public class BDSBackup
{
    private readonly ILogger<BDSBackup> _logger;
    private readonly IConfiguration _configuration;
    private readonly DirectoryIO _directoryIO;
    private readonly ServerProperties _serverProperties;
    private readonly OptionsIO _optionsIO;
    private readonly string? _serversPath;
    private readonly string? _backupsPath;

    public BDSBackup(ILogger<BDSBackup> logger, IConfiguration configuration, DirectoryIO directoryIO, ServerProperties serverProperties, OptionsIO optionsIO)
    {
        _logger = logger;
        _configuration = configuration;
        _directoryIO = directoryIO;
        _serverProperties = serverProperties;
        _optionsIO = optionsIO;
        _serversPath = _configuration["ServersPath"];
        _backupsPath = _configuration["BackupsPath"];
    }

    public Task Backup(ServerModel server)
    {
        if(string.IsNullOrEmpty(server.Path))
            throw new Exception("Server path is not set");
        
        if(string.IsNullOrEmpty(_serversPath))
            throw new Exception("ServersPath is not set in appsettings.json");

        if(string.IsNullOrEmpty(_backupsPath))
            throw new Exception("BackupsPath is not set in appsettings.json");

        var backupPath = Path.Combine(_backupsPath, server.Path);
        if (!Directory.Exists(backupPath))
            Directory.CreateDirectory(backupPath);

        var backupName = $"FULL_BACKUP_{DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss")}";
        var backupDirectory = Path.Combine(backupPath, backupName);
        Directory.CreateDirectory(backupDirectory);

        var files = Directory.GetFiles(Path.Combine(_serversPath, server.Path));
        foreach (var file in files)
        {
            var fileName = Path.GetFileName(file);

            
            if (fileName == null)
                continue;

            var backupFile = false;
            if(fileName == null)
                continue;
            
            if(fileName == "server.properties" 
                || fileName == "permissions.json" 
                || fileName == "allowlist.json" 
                || fileName == "known_valid_packs.json"
                || fileName == "world_behavior_packs.json"
                || fileName == "world_resource_packs.json"
                || fileName == "players.json"
                || fileName == "backup.json"
                || fileName == "update.json"
                || fileName == "autostart.json")
                backupFile = true;

            if(!backupFile)
                continue;

            File.Copy(file, Path.Combine(backupDirectory, fileName));
        }

        var directories = Directory.GetDirectories(Path.Combine(_serversPath, server.Path));
        foreach(var directory in directories)
        {
            var dirName = Path.GetFileName(directory);
            if(dirName == null)
                continue;

            var backupDir = false;
            if(dirName == "worlds"
                || dirName == "behavior_packs"
                || dirName == "resource_packs")
                backupDir = true;

            if(!backupDir)
                continue;

            _directoryIO.Copy(directory, Path.Combine(backupDirectory, dirName), true);
        }
        server.Backup.LastBackup = DateTime.Now;
        _serverProperties.SaveBackupSettings(server);
        _optionsIO.RefreshServers();
        return Task.CompletedTask;
    }

    private void CreateZipFromDirectory(string sourceDirectory, string destinationFile)
    {
        using var zip = ZipFile.Open(destinationFile, ZipArchiveMode.Create);

        foreach (var file in Directory.GetFiles(sourceDirectory, "*", SearchOption.AllDirectories))
        {
            string relativePath = file.Substring(sourceDirectory.Length + 1);

            zip.CreateEntryFromFile(file, relativePath);
        }
    }

    internal Task ArchiveBackup(ServerModel server)
    {
        if(string.IsNullOrEmpty(_backupsPath))
            throw new Exception("BackupsPath is not set in appsettings.json");
        var backupPath = Path.Combine(_backupsPath, server.Path);
        if (!Directory.Exists(backupPath))
            return Task.CompletedTask;

        foreach(var dir in Directory.GetDirectories(backupPath))
        {
            var dirName = Path.GetFileName(dir);
            if(dirName == null || dirName == Path.GetFileName(backupPath))
                continue;
            var sourceDirectory = Path.Combine(backupPath, dirName);
            var desitnationFile = Path.Combine(backupPath, $"{dirName}.zip");
            CreateZipFromDirectory(sourceDirectory, desitnationFile);
            Directory.Delete(sourceDirectory, true);
            DeleteOlderArchivedBackups(backupPath, server.Backup.BackupKeepCount);
        }
        return Task.CompletedTask;
    }

    private void DeleteOlderArchivedBackups(string path, int maxBackups)
    {
        var files = Directory.GetFiles(path);
        if (files.Length <= maxBackups)
            return;

        var filesToDelete = files.Length - maxBackups;
        var filesToDeleteList = files.OrderBy(f => f).Take(filesToDelete).ToList();
        foreach (var file in filesToDeleteList)
        {
            File.Delete(file);
        }
    }

    public Task RestoreBackup(ServerModel server, string archiveFileName, bool restoreWorldOnly = true)
    {
        if(string.IsNullOrEmpty(_backupsPath))
            throw new Exception("BackupsPath is not set in appsettings.json");
        if(string.IsNullOrEmpty(_serversPath))
            throw new Exception("ServersPath is not set in appsettings.json");
        var backupPath = Path.Combine(_backupsPath, server.Path);
        if (!Directory.Exists(backupPath))
            return Task.CompletedTask;

        var archivePath = Path.Combine(backupPath, archiveFileName);
        if (!File.Exists(archivePath))
            return Task.CompletedTask;

        var backupDirectory = Path.Combine(backupPath, Path.GetFileNameWithoutExtension(archiveFileName));
        if (Directory.Exists(backupDirectory))
            Directory.Delete(backupDirectory, true);
        Directory.CreateDirectory(backupDirectory);

        ZipFile.ExtractToDirectory(archivePath, backupDirectory);

        if(!restoreWorldOnly)
        {
            var files = Directory.GetFiles(backupDirectory);
            foreach (var file in files)
            {
                var fileName = Path.GetFileName(file);
                if(fileName == null)
                    continue;
                var destinationFile = Path.Combine(_serversPath, server.Path, fileName);
                if (File.Exists(destinationFile))
                    File.Delete(destinationFile);
                File.Move(file, destinationFile);
            }
        }

        var directories = Directory.GetDirectories(backupDirectory);
        foreach (var directory in directories)
        {
            var dirName = Path.GetFileName(directory);
            if(dirName == null)
                continue;

            if(dirName != "worlds" && restoreWorldOnly)
                continue;
            var destinationDirectory = Path.Combine(_serversPath, server.Path, dirName);
            if (Directory.Exists(destinationDirectory))
                Directory.Delete(destinationDirectory, true);
            Directory.Move(directory, destinationDirectory);
        }

        Directory.Delete(backupDirectory, true);
        return Task.CompletedTask;
    }
}