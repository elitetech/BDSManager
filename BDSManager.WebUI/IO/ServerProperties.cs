using BDSManager.WebUI.Models;
using Newtonsoft.Json;

namespace BDSManager.WebUI.IO;

public class ServerProperties
{
    private readonly IConfiguration _configuration;
    private readonly string _serversPath = "";

    public ServerProperties(IConfiguration configuration)
    {
        _configuration = configuration;
        if(!string.IsNullOrEmpty(_configuration["ServersPath"]))
            _serversPath = _configuration["ServersPath"];
    }

    private ServerModel GetServer(string path)
    {
        var server = new ServerModel();
        server.Path = path;
        server.Icon = "https://minecraft.net/favicon.ico";
        server.Version = GetVersion(server);
        server.Options = ParseServerProperties(server);
        server.AllowList = ParseAllowList(server);
        server.Permissions = ParsePermissions(server);
        return server;
    }

    public List<ServerModel> GetServers()
    {
        if(string.IsNullOrEmpty(_serversPath))
            throw new Exception("Servers path not set in configuration");

        var servers = new List<ServerModel>();
        Directory.GetDirectories(_serversPath).ToList().ForEach(serverPath =>
        {
            if(string.IsNullOrEmpty(serverPath))
                return;

            var serverDirectoryName = Path.GetFileName(serverPath);
            if(string.IsNullOrEmpty(serverDirectoryName))
                return;

            servers.Add(GetServer(serverDirectoryName));
        });
        return servers;
    }

    private string GetVersion(ServerModel server)
    {
        var versionPath = Path.Combine(_serversPath, server.Path, "version.txt");
        var version = File.Exists(versionPath) ? File.ReadAllText(versionPath) : "";
        return version;
    }

    private ServerOptionsModel ParseServerProperties(ServerModel server)
    {
        var serverPropertiesPath = Path.Combine(_serversPath, server.Path,"server.properties");
        var options = new ServerOptionsModel();
        if(!File.Exists(serverPropertiesPath))
            return options;
            
        File.ReadAllLines(serverPropertiesPath).ToList().ForEach(line =>
        {
            var split = line.Split('=');
            if (split.Length != 2)
                return;
            var key = split[0];
            var value = split[1];
            switch (key)
            {
                case "server-name":
                    options.Name = value;
                    break;
                case "server-port":
                    options.Port = value;
                    break;
                case "server-portv6":
                    options.Portv6 = value;
                    break;
                case "max-players":
                    options.MaxPlayers = value;
                    break;
                case "gamemode":
                    options.Gamemode = value;
                    break;
                case "difficulty":                  
                    options.Difficulty = value;
                    break;
                case "allow-cheats":
                    options.AllowCheats = value;
                    break;
                case "online-mode":
                    options.OnlineMode = value;
                    break;
                case "white-list":
                    options.AllowList = value;
                    break;
                case "max-threads":
                    options.MaxThreads = value;
                    break;
                case "view-distance":
                    options.ViewDistance = value;
                    break;
                case "tick-distance":
                    options.TickDistance = value;
                    break;
                case "player-idle-timeout":
                    options.PlayerIdleTimeout = value;
                    break;
                case "level-name":
                    options.LevelName = value;
                    break;
                case "level-seed":
                    options.LevelSeed = value;
                    break;
                case "compression-threshold":
                    options.CompressionThreshold = value;
                    break;
                case "default-player-permission-level":
                    options.DefaultPlayerPermissionLevel = value;
                    break;
                case "texturepack-required":
                    options.TexturePackRequired = value;
                    break;
                case "content-log-file-enabled":
                    options.ContentLog = value;
                    break;
                case "force-gamemode":
                    options.ForceGamemode = value;
                    break;
                case "server-authoritative-movement":
                    options.ServerAuthoritativeMovement = value;
                    break;
                case "player-movement-score-threshold":
                    options.PlayerMovementScoreThreshold = value;
                    break;
                case "player-movement-distance-threshold":
                    options.PlayerMovementDistanceThreshold = value;
                    break;
                case "player-movement-duration-threshold-in-ms":
                    options.PlayerMovementDurationThresholdInMs = value;
                    break;
                case "player-movement-action-direction-threshold":
                    options.PlayerMovementActionDirectionThreshold = value;
                    break;
                case "correct-player-movement":
                    options.CorrectPlayerMovement = value;
                    break;
                case "server-authoritative-block-breaking":
                    options.ServerAuthoritativeBlockBreaking = value;
                    break;
            }
        });
        return options;
    }

    public void SaveServerProperties(ServerModel server)
    {
        var path = Path.Combine(_serversPath, server.Path, "server.properties");
        var lines = new List<string>();
        lines.Add($"server-name={server.Options.Name}");
        lines.Add($"server-port={server.Options.Port}");
        lines.Add($"server-portv6={server.Options.Portv6}");
        lines.Add($"max-players={server.Options.MaxPlayers}");
        lines.Add($"gamemode={server.Options.Gamemode}");
        lines.Add($"difficulty={server.Options.Difficulty}");
        lines.Add($"allow-cheats={server.Options.AllowCheats}");
        lines.Add($"online-mode={server.Options.OnlineMode}");
        lines.Add($"white-list={server.Options.AllowList}");
        lines.Add($"max-threads={server.Options.MaxThreads}");
        lines.Add($"view-distance={server.Options.ViewDistance}");
        lines.Add($"tick-distance={server.Options.TickDistance}");
        lines.Add($"player-idle-timeout={server.Options.PlayerIdleTimeout}");
        lines.Add($"level-name={server.Options.LevelName}");
        lines.Add($"level-seed={server.Options.LevelSeed}");
        lines.Add($"compression-threshold={server.Options.CompressionThreshold}");
        lines.Add($"default-player-permission-level={server.Options.DefaultPlayerPermissionLevel}");
        lines.Add($"texturepack-required={server.Options.TexturePackRequired}");
        lines.Add($"content-log-file-enabled={server.Options.ContentLog}");
        lines.Add($"force-gamemode={server.Options.ForceGamemode}");
        lines.Add($"server-authoritative-movement={server.Options.ServerAuthoritativeMovement}");
        lines.Add($"player-movement-score-threshold={server.Options.PlayerMovementScoreThreshold}");
        lines.Add($"player-movement-distance-threshold={server.Options.PlayerMovementDistanceThreshold}");
        lines.Add($"player-movement-duration-threshold-in-ms={server.Options.PlayerMovementDurationThresholdInMs}");
        lines.Add($"player-movement-action-direction-threshold={server.Options.PlayerMovementActionDirectionThreshold}");
        lines.Add($"correct-player-movement={server.Options.CorrectPlayerMovement}");
        lines.Add($"server-authoritative-block-breaking={server.Options.ServerAuthoritativeBlockBreaking}");
        File.WriteAllLines(path, lines);
    }

    private List<PermissionModel> ParsePermissions(ServerModel server)
    {
        var permissionsPath = Path.Combine(_serversPath, server.Path, "permissions.json");

        return File.Exists(permissionsPath) ? JsonConvert.DeserializeObject<List<PermissionModel>>(File.ReadAllText(permissionsPath)) : new List<PermissionModel>();
    }

    public void SavePermissions(ServerModel server)
    {
        var path = Path.Combine(_serversPath, server.Path, "permissions.json");
        var serializedPermissions = JsonConvert.SerializeObject(server.Permissions);
        File.WriteAllText(path, serializedPermissions);
    }

    private List<AllowPlayerModel> ParseAllowList(ServerModel server)
    {
        var allowListPath = Path.Combine(_serversPath, server.Path, "allowlist.json");

        return File.Exists(allowListPath) ? JsonConvert.DeserializeObject<List<AllowPlayerModel>>(File.ReadAllText(allowListPath)) : new List<AllowPlayerModel>();
    }

    public void SaveAllowList(ServerModel server)
    {
        var path = Path.Combine(_serversPath, server.Path, "allowlist.json");
        var serializedAllowList = JsonConvert.SerializeObject(server.AllowList);
        File.WriteAllText(path, serializedAllowList);
    }
}