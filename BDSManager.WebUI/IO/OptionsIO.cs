
using BDSManager.WebUI.Models;
using Newtonsoft.Json;

namespace BDSManager.WebUI.IO;

public class OptionsIO
{
    private readonly string _filepath = "manager.json";
    public ManagerOptionsModel ManagerOptions = new();
    public bool FirstSetup = false;

    public OptionsIO()
    {
        if (!File.Exists(_filepath))
            SaveOptionsFile();
        ReadOptionsFile();
    }

    public void SaveOptionsFile()
    {
        string json = JsonConvert.SerializeObject(ManagerOptions, Formatting.Indented);
        File.WriteAllText(_filepath, json);
    }

    internal void AddServer(ServerModel server)
    {
        ManagerOptions.Servers.Add(server);
        SaveOptionsFile();
        ReadOptionsFile();
    }

    internal void RemoveServer(ServerModel server)
    {
        ManagerOptions.Servers.Remove(server);
        SaveOptionsFile();
        ReadOptionsFile();
    }

    private void ReadOptionsFile()
    {
        string json = File.ReadAllText(_filepath);
        var _managerOptions = JsonConvert.DeserializeObject<ManagerOptionsModel>(json);
        FirstSetup = _managerOptions.Servers.Any() ? false : true;
    }
}