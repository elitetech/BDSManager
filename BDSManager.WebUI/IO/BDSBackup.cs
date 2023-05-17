using System.IO.Compression;
using BDSManager.WebUI.Hubs;
using BDSManager.WebUI.Models;
using BDSManager.WebUI.Services;

namespace BDSManager.WebUI.IO;

public class BDSBackup
{
    private readonly ILogger<BDSBackup> _logger;
    private readonly IConfiguration _configuration;
    private readonly DirectoryIO _directoryIO;
    private readonly ServerProperties _serverProperties;
    private readonly OptionsIO _optionsIO;
    private readonly ConsoleHub _consoleHub;
    private readonly string? _serversPath;
    private readonly string? _backupsPath;
    private const int MAX_QUERY_TRIES = 10;

    public BDSBackup(ILogger<BDSBackup> logger, IConfiguration configuration, DirectoryIO directoryIO, ServerProperties serverProperties, OptionsIO optionsIO, ConsoleHub consoleHub)
    {
        _logger = logger;
        _configuration = configuration;
        _directoryIO = directoryIO;
        _serverProperties = serverProperties;
        _optionsIO = optionsIO;
        _consoleHub = consoleHub;
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
                || fileName == "server.json")
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
        server.Backup.NextBackup = DateTime.Now.AddHours(server.Backup.BackupInterval);
        _serverProperties.SaveServerSettings(server);
        _optionsIO.RefreshServers();
        return Task.CompletedTask;
    }

    public async Task WorldBackup(string serverPath, string fileList)
    {
        if(string.IsNullOrEmpty(_backupsPath))
            throw new Exception("BackupsPath is not set in appsettings.json");
        if(string.IsNullOrEmpty(_serversPath))
            throw new Exception("ServersPath is not set in appsettings.json");

        var backupPath = Path.Combine(_backupsPath, serverPath);
        if (!Directory.Exists(backupPath))
            Directory.CreateDirectory(backupPath);

        var server = _optionsIO.ManagerOptions.Servers.FirstOrDefault(x => x.Path == serverPath);
        if(server == null)
            return;

        var backupName = $"WORLD_BACKUP_{DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss")}";
        var backupWorldDirectory = Path.Combine(backupPath, backupName);
        Directory.CreateDirectory(backupWorldDirectory);
        var directory = Path.Combine(_serversPath, serverPath, "worlds");

        var filesToBackup = fileList.Split(',');
        foreach(var file in filesToBackup)
        {
            var parts = file.Split(':');
            var fileName = parts[0].Trim();
            var trimSize = parts[1];
            if(string.IsNullOrEmpty(fileName) || string.IsNullOrEmpty(trimSize))
                continue;
            var destinationFile = Path.Combine(backupWorldDirectory, fileName);
            // get directory path of destination file and create if it doesn't exist
            var destinationDirectory = Path.GetDirectoryName(destinationFile);
            if(!Directory.Exists(destinationDirectory))
                Directory.CreateDirectory(destinationDirectory);
            File.Copy(Path.Combine(directory, fileName), destinationFile);

            // trim file size in bytes to trimSize
            var trimSizeBytes = int.Parse(trimSize);
            var fileInfo = new FileInfo(Path.Combine(backupWorldDirectory, fileName));
            if(fileInfo.Length > trimSizeBytes)
            {
                var fileBytes = File.ReadAllBytes(Path.Combine(backupWorldDirectory, fileName));
                var neededBytes = fileBytes.SkipLast(fileBytes.Length - trimSizeBytes).ToArray();
                File.WriteAllBytes(Path.Combine(backupWorldDirectory, fileName), neededBytes);
            }
        }

        
        server.Backup.LastWorldBackup = DateTime.Now;
        server.Backup.NextWorldBackup = DateTime.Now.AddHours(server.Backup.WorldBackupInterval);
        _serverProperties.SaveServerSettings(server);
        _optionsIO.RefreshServers();
        await ArchiveBackup(server);
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
            DeleteOlderArchivedBackups(server);
        }
        _consoleHub.UpdateConsoleOutput(server.Path, $"Archived backups for {server.Options.Name}");
        return Task.CompletedTask;
    }

    private void DeleteOlderArchivedBackups(ServerModel server)
    {
        if(string.IsNullOrEmpty(_backupsPath))
            throw new Exception("BackupsPath is not set in appsettings.json");
        var path = Path.Combine(_backupsPath, server.Path);
        var files = Directory.GetFiles(path);
        var worldBackupFiles = files.Where(x => x.Contains("WORLD_BACKUP_")).ToList();
        var fullBackupFiles = files.Where(x => x.Contains("FULL_BACKUP_")).ToList();
        
        if(worldBackupFiles.Count > server.Backup.WorldBackupKeepCount)
        {
            var filesToDelete = worldBackupFiles.Count - server.Backup.WorldBackupKeepCount;
            var filesToDeleteList = worldBackupFiles.OrderBy(f => f).Take(filesToDelete).ToList();
            foreach (var file in filesToDeleteList)
            {
                File.Delete(file);
            }
        }

        if(fullBackupFiles.Count > server.Backup.BackupKeepCount)
        {
            var filesToDelete = fullBackupFiles.Count - server.Backup.BackupKeepCount;
            var filesToDeleteList = fullBackupFiles.OrderBy(f => f).Take(filesToDelete).ToList();
            foreach (var file in filesToDeleteList)
            {
                File.Delete(file);
            }
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

        _consoleHub.UpdateConsoleOutput(server.Path, $"Restoring backup {archiveFileName} for {server.Options.Name}");
        
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
        _consoleHub.UpdateConsoleOutput(server.Path, $"Restored backup {archiveFileName} for {server.Options.Name}");
        return Task.CompletedTask;
    }

    internal void TrimWorldBackup(string path)
    {
        throw new NotImplementedException();
    }
}