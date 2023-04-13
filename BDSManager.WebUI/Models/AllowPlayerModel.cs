namespace BDSManager.WebUI.Models;

public class AllowPlayerModel
{
    public bool ignoresPlayerLimit { get; set; } = false;
    public string? name { get; set; }
    public string? xuid { get; set; }
}