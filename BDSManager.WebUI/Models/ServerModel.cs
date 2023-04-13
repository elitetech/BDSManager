namespace BDSManager.WebUI.Models;

public class ServerModel
{
    public string Path { get; set; } = "00";
    public string Icon { get; set; } = "https://minecraft.net/favicon.ico";
    public string? Version { get; set; }
    public ServerOptionsModel Options { get; set; } = new();
    public List<AllowPlayerModel> AllowList { get; set; } = new();
    public List<PermissionModel> Permissions { get; set; } = new();
}