using System.IO.Compression;
using BDSManager.WebUI.Models;

namespace BDSManager.WebUI.IO;

public class BDSBackup
{
    private readonly ILogger<BDSBackup> _logger;
    private readonly IConfiguration _configuration;
    private readonly string? _serversPath;
    private readonly string? _backupsPath;

    public BDSBackup(ILogger<BDSBackup> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
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
                || fileName == "update.json")
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

            new DirectoryCopy().Copy(directory, Path.Combine(backupDirectory, dirName), true);
        }
        CreateZipFromDirectory(backupPath, backupName, backupDirectory);
        DeleteDirectory(backupDirectory);
        DeleteOlderArchivedBackups(backupPath, server.Backup.BackupKeepCount);
        return Task.CompletedTask;
    }

    private void CreateZipFromDirectory(string backupPath, string backupName, string backupDirectory)
    {
        string zipFileName = Path.Combine(backupPath, $"{backupName}.zip");

        // Create a new zip archive
        using var zip = ZipFile.Open(zipFileName, ZipArchiveMode.Create);

        // Iterate through the files in the directory
        foreach (var file in Directory.GetFiles(backupDirectory, "*", SearchOption.AllDirectories))
        {
            // Get the relative path of the file within the backupDirectory
            string relativePath = file.Substring(backupDirectory.Length + 1);

            // Add the file to the zip archive
            zip.CreateEntryFromFile(file, relativePath);
        }
    }

    private void DeleteDirectory(string path)
    {
        foreach (var directory in Directory.GetDirectories(path))
        {
            DeleteDirectory(directory);
        }

        foreach (var file in Directory.GetFiles(path))
        {
            File.Delete(file);
        }

        Directory.Delete(path);
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
}