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
        _serversPath = _configuration["ServersPath"] ?? string.Empty;
    }

    private ServerModel GetServer(string path)
    {
        var server = new ServerModel();
        server.Path = path;
        server.Icon = "https://minecraft.net/favicon.ico";
        server = ParseServerSettings(server);
        return server;
    }

    internal List<string> ListWorlds(ServerModel server)
    {
        var worlds = new List<string>();
        var worldsPath = Path.Combine(_serversPath, server.Path, "worlds");
        if(!Directory.Exists(worldsPath))
            return worlds;
        
        Directory.GetDirectories(worldsPath).ToList().ForEach(worldPath =>
        {
            if(string.IsNullOrEmpty(worldPath))
                return;

            var worldDirectoryName = Path.GetFileName(worldPath);
            if(string.IsNullOrEmpty(worldDirectoryName))
                return;

            if(worldDirectoryName != "worlds")
                worlds.Add(worldDirectoryName);
        });
        return worlds;
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

    private ServerModel ParseServerSettings(ServerModel server)
    {
        var serverSettingsPath = Path.Combine(_serversPath, server.Path, "server.json");
        if(!File.Exists(serverSettingsPath))
            return server;

        var serverSettingsJson = File.ReadAllText(serverSettingsPath);
        var serverSettings = JsonConvert.DeserializeObject<ServerModel>(serverSettingsJson);
        if(serverSettings is null)
            return server;

        server = serverSettings;
        server.Addons = ParseAddons(server);
        server.Worlds = ListWorlds(server);
        return server;
    }

    public void SaveServerSettings(ServerModel server)
    {
        var serverSettingsPath = Path.Combine(_serversPath, server.Path, "server.json");
        var serverSettingsJson = JsonConvert.SerializeObject(server, Formatting.Indented);
        File.WriteAllText(serverSettingsPath, serverSettingsJson);

        SaveServerProperties(server);
        SaveAllowList(server);
        SavePermissions(server);
        SaveResourcePacks(server);
        SaveBehaviorPacks(server);
    }

    private void SaveServerProperties(ServerModel server)
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
        lines.Add($"emit-server-telemetry={server.Options.EmitServerTelemetry}");
        File.WriteAllLines(path, lines);
    }

    private void SavePermissions(ServerModel server)
    {
        var path = Path.Combine(_serversPath, server.Path, "permissions.json");
        var serializedPermissions = JsonConvert.SerializeObject(server.Permissions);
        File.WriteAllText(path, serializedPermissions);
    }

    private void SaveAllowList(ServerModel server)
    {
        var path = Path.Combine(_serversPath, server.Path, "allowlist.json");
        var serializedAllowList = JsonConvert.SerializeObject(server.AllowList);
        File.WriteAllText(path, serializedAllowList);
    }

    private List<WorldPackModel> ParseWorldResourcePacks(ServerModel server)
    {
        var worldResourcePacksPath = Path.Combine(_serversPath, server.Path, "worlds", server.Options.LevelName, "world_resource_packs.json");

        return File.Exists(worldResourcePacksPath) ? JsonConvert.DeserializeObject<List<WorldPackModel>>(File.ReadAllText(worldResourcePacksPath)) : new List<WorldPackModel>();
    }

    private void SaveResourcePacks(ServerModel server)
    {
        var path = Path.Combine(_serversPath, server.Path, "worlds", server.Options.LevelName, "world_resource_packs.json");
        var addons = server.Addons
            .Where(x => x.Manifest.modules.Any(y => y.type == "resources"))
            .Select(x => new WorldPackModel { pack_id = x.Manifest.header.uuid, version = x.Manifest.header.version })
            .ToList();
        var serializedResourcePacks = JsonConvert.SerializeObject(addons);
        
        if(!Directory.Exists(Path.Combine(_serversPath, server.Path, "worlds", server.Options.LevelName)))
            Directory.CreateDirectory(Path.Combine(_serversPath, server.Path, "worlds", server.Options.LevelName));
        
        File.WriteAllText(path, serializedResourcePacks);
    }

    private List<WorldPackModel> ParseBehaviorResourcePacks(ServerModel server)
    {
        var behaviorResourcePacksPath = Path.Combine(_serversPath, server.Path, "worlds", server.Options.LevelName, "world_behavior_packs.json");

        return File.Exists(behaviorResourcePacksPath) ? JsonConvert.DeserializeObject<List<WorldPackModel>>(File.ReadAllText(behaviorResourcePacksPath)) : new List<WorldPackModel>();
    }

    private void SaveBehaviorPacks(ServerModel server)
    {
        var path = Path.Combine(_serversPath, server.Path, "worlds", server.Options.LevelName, "world_behavior_packs.json");
        var addons = server.Addons
            .Where(x => x.Manifest.modules.Any(y => y.type != "resources"))
            .Select(x => new WorldPackModel { pack_id = x.Manifest.header.uuid, version = x.Manifest.header.version })
            .ToList();
        var serializedBehaviorPacks = JsonConvert.SerializeObject(addons);
        
        if(!Directory.Exists(Path.Combine(_serversPath, server.Path, "worlds", server.Options.LevelName)))
            Directory.CreateDirectory(Path.Combine(_serversPath, server.Path, "worlds", server.Options.LevelName));

        File.WriteAllText(path, serializedBehaviorPacks);
    }

    private List<AddonPackModel> ParseAddons(ServerModel server)
    {
        var behaviorPacks = ParseBehaviorResourcePacks(server);
        var resourcePacks = ParseWorldResourcePacks(server);

        var addons = new List<AddonPackModel>();

        addons.AddRange(GetAddonPacks(Path.Combine(_serversPath, server.Path, "behavior_packs"), behaviorPacks));
        addons.AddRange(GetAddonPacks(Path.Combine(_serversPath, server.Path, "resource_packs"), resourcePacks));

        return addons;
    }

    private List<AddonPackModel> GetAddonPacks(string path, List<WorldPackModel> worldPacks)
    {
        var addons = new List<AddonPackModel>();
        if(!Directory.Exists(path))
            return addons;
        
        foreach(var addonRoot in Directory.GetDirectories(path))
        {
            var addonRootName = Path.GetFileName(addonRoot);
            if (addonRootName == "behavior_packs" || addonRootName == "resource_packs")
                continue;
            
            
            var manifestPath = Path.Combine(addonRoot, "manifest.json");
            foreach(var directory in Directory.GetDirectories(addonRoot))
            {
                if (!File.Exists(manifestPath))
                    manifestPath = Path.Combine(directory, "manifest.json");
                if (!File.Exists(manifestPath))
                    continue;
                var manifest = JsonConvert.DeserializeObject<ManifestModel>(File.ReadAllText(manifestPath));

                if(!worldPacks.Any(x => x.pack_id == manifest.header.uuid))
                    continue;

                addons.Add(new AddonPackModel
                {
                    Path = directory,
                    Manifest = manifest
                });
            }
        }
        return addons;
    }
}