namespace BDSManager.WebUI.Models;

public class ServerModel
{
    public string Path { get; set; } = string.Empty;
    public string Icon { get; set; } = "https://minecraft.net/favicon.ico";
    public string Online { get; set; } = "Offline";
    public string Players { get; set; } = "0/0";
    public string? Version { get; set; }
    public ServerOptionsModel Options { get; set; } = new();
    public List<AllowPlayerModel> AllowList { get; set; } = new();
    public List<PermissionModel> Permissions { get; set; } = new();
}