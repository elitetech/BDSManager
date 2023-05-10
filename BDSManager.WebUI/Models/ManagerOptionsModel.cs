using BDSManager.WebUI.Services;

namespace BDSManager.WebUI.Models;

public class ManagerOptionsModel
{
    public List<ServerModel> Servers { get; set; } = new();
    public List<IMinecraftCommand> Commands { get; set; } = new();
}

