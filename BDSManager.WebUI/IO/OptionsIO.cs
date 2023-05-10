
using BDSManager.WebUI.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BDSManager.WebUI.IO;

public class OptionsIO
{
    private readonly IConfiguration _configuration;
    private readonly ServerProperties _serverProperties;
    private readonly string _serversPath = "";
    public ManagerOptionsModel ManagerOptions = new();
    public bool FirstSetup = false;

    public OptionsIO(IConfiguration configuration, ServerProperties serverProperties)
    {
        _configuration = configuration;
        _serverProperties = serverProperties;
        _serversPath = _configuration["ServersPath"] ?? string.Empty;
        
        CheckServersDirectoryForServers();
        GetCommands();
        UpdateFirstRun();
    }

    private void GetCommands()
    {
        // get commands from commands.json
        var commandsPath = Path.Combine("wwwroot", "commands.json");
        if (!File.Exists(commandsPath))
            throw new Exception("commands.json not found");

        string fileContent = File.ReadAllText(commandsPath);

        // Parse the content into a JArray
        JArray jsonArray = JArray.Parse(fileContent);
        
        var commandsJson = jsonArray.Select(item => item.ToString(Formatting.None)).ToArray();

        foreach(var commandJson in commandsJson)
        {
            var commandBase = JsonConvert.DeserializeObject<MinecraftCommandBase>(commandJson);
            if(commandBase is null)
                throw new Exception("Invalid command in commands.json");
            if(commandBase.Name == "give")
            {
                var giveCommand = JsonConvert.DeserializeObject<MinecraftGiveCommandModel>(commandJson);
                if(giveCommand is not null)
                    ManagerOptions.Commands.Add(giveCommand);
            }

            if(commandBase.Name == "effect")
            {
                var effectCommand = JsonConvert.DeserializeObject<MinecraftEffectCommandModel>(commandJson);
                if(effectCommand is not null)
                    ManagerOptions.Commands.Add(effectCommand);
            }

            if(commandBase.Name == "enchant")
            {
                var enchantCommand = JsonConvert.DeserializeObject<MinecraftEnchantmentCommandModel>(commandJson);
                if(enchantCommand is not null)
                    ManagerOptions.Commands.Add(enchantCommand);
            }

            if(commandBase.Name == "time")
            {
                var timeCommand = JsonConvert.DeserializeObject<MinecraftTimeCommandModel>(commandJson);
                if(timeCommand is not null)
                    ManagerOptions.Commands.Add(timeCommand);
            }

            if(commandBase.Name == "weather")
            {
                var weatherCommand = JsonConvert.DeserializeObject<MinecraftWeatherCommandModel>(commandJson);
                if(weatherCommand is not null)
                    ManagerOptions.Commands.Add(weatherCommand);
            }

            if(commandBase.Name == "teleport")
            {
                var teleportCommand = JsonConvert.DeserializeObject<MinecraftTeleportCommandModel>(commandJson);
                if(teleportCommand is not null)
                    ManagerOptions.Commands.Add(teleportCommand);
            }
        }
    }

    internal void AddServer(ServerModel server)
    {
        if(!ManagerOptions.Servers.Contains(server))
            ManagerOptions.Servers.Add(server);
        UpdateFirstRun();
        
    }

    internal void RemoveServer(ServerModel server)
    {
        ManagerOptions.Servers.Remove(server);
        UpdateFirstRun();
    }

    internal void RefreshServers()
    {
        ManagerOptions.Servers.Clear();
        CheckServersDirectoryForServers();
        UpdateFirstRun();
    }

    private void UpdateFirstRun() => FirstSetup = ManagerOptions.Servers.Any() ? false : true;

    private void CheckServersDirectoryForServers()
    {
        if (string.IsNullOrEmpty(_serversPath))
            throw new Exception("Servers path not set in configuration");

        if (!Directory.Exists(_serversPath))
            Directory.CreateDirectory(_serversPath);

        Directory.GetDirectories(_serversPath).ToList().ForEach(serverPath =>
        {
            var servers = _serverProperties.GetServers();
            ManagerOptions.Servers.AddRange(servers.Where(x => !ManagerOptions.Servers.Any(y => y.Path == x.Path)));
        });
    }
}