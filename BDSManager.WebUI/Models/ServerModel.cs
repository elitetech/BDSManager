namespace BDSManager.WebUI.Models;

public class ServerModel
{
    public string Path { get; set; } = string.Empty;
    public string Icon { get; set; } = "https://www.minecraft.net/etc.clientlibs/minecraft/clientlibs/main/resources/favicon.ico";
    public string Online { get; set; } = "Offline";
    public string? Version { get; set; }
    public List<string> Worlds { get; set; } = new();
    public UpdateModel Update { get; set; } = new();
    public BackupModel Backup { get; set; } = new();
    public bool AutoStartEnabled { get; set; } = false;
    public DateTime? LastStarted { get; set; }
    public List<PlayerModel> Players { get; set; } = new();
    public ServerOptionsModel Options { get; set; } = new();
    public List<AllowPlayerModel> AllowList { get; set; } = new();
    public List<PermissionModel> Permissions { get; set; } = new();
    public List<AddonPackModel> Addons { get; set; } = new();
}