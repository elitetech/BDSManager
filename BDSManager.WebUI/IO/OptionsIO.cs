
using BDSManager.WebUI.Models;
using Newtonsoft.Json;

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
        GetItems();
        UpdateFirstRun();
    }

    private void GetItems()
    {
        // get items from items.json
        var itemsPath = Path.Combine("wwwroot", "items.json");
        if (!File.Exists(itemsPath))
            throw new Exception("items.json not found");
        
        ManagerOptions.Items = JsonConvert.DeserializeObject<List<ItemModel>>(File.ReadAllText(itemsPath)).Where(x => !x.IDName.Contains("element_")).ToList();
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