
using BDSManager.WebUI.Models;
using Newtonsoft.Json;

namespace BDSManager.WebUI.IO;

public class OptionsIO
{
    private readonly string _filepath = "manager.json";
    public bool FirstSetup = false;

    public ManagerOptionsModel LoadOptions()
    {
        if (!File.Exists(_filepath))
        {
            SaveOptionsFile(new ManagerOptionsModel());
        }
        return ReadOptionsFile();
    }

    public void SaveOptionsFile(ManagerOptionsModel options)
    {
        string json = JsonConvert.SerializeObject(options, Formatting.Indented);
        File.WriteAllText(_filepath, json);
    }

    private ManagerOptionsModel ReadOptionsFile()
    {
        string json = File.ReadAllText(_filepath);
        var options = JsonConvert.DeserializeObject<ManagerOptionsModel>(json);
        if (options.Servers.Count == 0)
            FirstSetup = true;
        return options;
    }
}