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
        server.Version = "";
        server.Options = new ServerOptionsModel();
        server.AllowList = new List<AllowPlayerModel>();
        server.Permissions = new List<PermissionModel>();
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

            var serverDirectoryName = Path.GetDirectoryName(serverPath);
            if(string.IsNullOrEmpty(serverDirectoryName))
                return;

            servers.Add(GetServer(serverDirectoryName));
        });
        return servers;
    }

    private void ParseServerProperties(ServerModel server)
    {
        File.ReadAllLines(Path.Combine(_serversPath, server.Path,"server.properties")).ToList().ForEach(line =>
        {
            var split = line.Split('=');
            if (split.Length != 2)
                return;
            var key = split[0];
            var value = split[1];
            switch (key)
            {
                case "server-name":
                    server.Options.Name = value;
                    break;
                case "server-port":
                    server.Options.Port = value;
                    break;
                case "server-portv6":
                    server.Options.Portv6 = value;
                    break;
                case "max-players":
                    server.Options.MaxPlayers = value;
                    break;
                case "gamemode":
                    server.Options.Gamemode = value;
                    break;
                case "difficulty":                  
                    server.Options.Difficulty = value;
                    break;
                case "allow-cheats":
                    server.Options.AllowCheats = value;
                    break;
                case "online-mode":
                    server.Options.OnlineMode = value;
                    break;
                case "white-list":
                    server.Options.AllowList = value;
                    break;
                case "max-threads":
                    server.Options.MaxThreads = value;
                    break;
                case "view-distance":
                    server.Options.ViewDistance = value;
                    break;
                case "tick-distance":
                    server.Options.TickDistance = value;
                    break;
                case "player-idle-timeout":
                    server.Options.PlayerIdleTimeout = value;
                    break;
                case "level-name":
                    server.Options.LevelName = value;
                    break;
                case "level-seed":
                    server.Options.LevelSeed = value;
                    break;
                case "compression-threshold":
                    server.Options.CompressionThreshold = value;
                    break;
                case "default-player-permission-level":
                    server.Options.DefaultPlayerPermissionLevel = value;
                    break;
                case "texturepack-required":
                    server.Options.TexturePackRequired = value;
                    break;
                case "content-log-file-enabled":
                    server.Options.ContentLog = value;
                    break;
                case "force-gamemode":
                    server.Options.ForceGamemode = value;
                    break;
                case "server-authoritative-movement":
                    server.Options.ServerAuthoritativeMovement = value;
                    break;
                case "player-movement-score-threshold":
                    server.Options.PlayerMovementScoreThreshold = value;
                    break;
                case "player-movement-distance-threshold":
                    server.Options.PlayerMovementDistanceThreshold = value;
                    break;
                case "player-movement-duration-threshold-in-ms":
                    server.Options.PlayerMovementDurationThresholdInMs = value;
                    break;
                case "player-movement-action-direction-threshold":
                    server.Options.PlayerMovementActionDirectionThreshold = value;
                    break;
                case "correct-player-movement":
                    server.Options.CorrectPlayerMovement = value;
                    break;
                case "server-authoritative-block-breaking":
                    server.Options.ServerAuthoritativeBlockBreaking = value;
                    break;
            }
        });
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

    private void ParsePermissions(ServerModel server)
    {
        var serializedPermissions = File.ReadAllText(Path.Combine(_serversPath, server.Path, "permissions.json"));
        var permissions = JsonConvert.DeserializeObject<List<PermissionModel>>(serializedPermissions);
        server.Permissions = permissions;
    }

    public void SavePermissions(ServerModel server)
    {
        var path = Path.Combine(_serversPath, server.Path, "permissions.json");
        var serializedPermissions = JsonConvert.SerializeObject(server.Permissions);
        File.WriteAllText(path, serializedPermissions);
    }

    private void ParseAllowList(ServerModel server)
    {
        var serializedAllowList = File.ReadAllText(Path.Combine(_serversPath, server.Path, "allowlist.json"));
        var allowList = JsonConvert.DeserializeObject<List<AllowPlayerModel>>(serializedAllowList);
        server.AllowList = allowList;
    }

    public void SaveAllowList(ServerModel server)
    {
        var path = Path.Combine(_serversPath, server.Path, "allowlist.json");
        var serializedAllowList = JsonConvert.SerializeObject(server.AllowList);
        File.WriteAllText(path, serializedAllowList);
    }
}