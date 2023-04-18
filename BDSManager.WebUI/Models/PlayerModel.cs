namespace BDSManager.WebUI.Models;

public class PlayerModel
{
    public string Name { get; set; } = string.Empty;
    public string XUID { get; set; } = string.Empty;
    public bool Online { get; set; } = false;
    public DateTime? LastSeen { get; set; }
}