using BDSManager.WebUI.Models;

namespace BDSManager.WebUI.IO;

public class BDSBackup
{
    private readonly ILogger<NewServerModel> _logger;
    private readonly IConfiguration _configuration;
    private readonly string? _serversPath;
    private readonly string? _backupsPath;

    public BDSBackup(ILogger<NewServerModel> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
        _serversPath = _configuration["ServersPath"];
        _backupsPath = _configuration["BackupsPath"];
    }

    public void Backup(ServerModel server)
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

        var backupName = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        var backupDir = Path.Combine(backupPath, backupName);
        Directory.CreateDirectory(backupDir);

        var files = Directory.GetFiles(Path.Combine(_serversPath, server.Path));
        foreach (var file in files)
        {
            var fileName = Path.GetFileName(file);

            
            if (fileName == null)
                continue;

            if(fileName == null
                || fileName != "server.properties" 
                || fileName != "permissions.json" 
                || fileName != "allowlist.json" 
                || fileName != "worlds"
                || fileName != "known_valid_packs.json"
                || fileName != "world_behavior_packs.json"
                || fileName != "world_resource_packs.json"
                || fileName != "behavior_packs"
                || fileName != "resource_packs")
                continue;

            if(fileName == "worlds")
            {
                var worlds = Directory.GetDirectories(Path.Combine(_serversPath, server.Path, "worlds"));
                foreach (var world in worlds)
                {
                    var worldName = Path.GetFileName(world);
                    if (worldName == null || worldName != server.Options.LevelName)
                        continue;

                    BackupWorld(Path.Combine(_serversPath, server.Path), worldName, backupDir);
                }
            }

            File.Copy(file, Path.Combine(backupDir, fileName));
        }
    }

    private void BackupWorld(string serverPath, string worldName, string backupPath)
    {
        var worldPath = Path.Combine(serverPath, "worlds", worldName);
        var backupWorldPath = Path.Combine(backupPath, "worlds", worldName);
        Directory.CreateDirectory(backupWorldPath);

        // TODO: Get file list from server service
         

        var files = Directory.GetFiles(worldPath);
        foreach (var file in files)
        {
            var fileName = Path.GetFileName(file);
            if (fileName == null)
                continue;

            File.Copy(file, Path.Combine(backupWorldPath, fileName));
        }
    }
}