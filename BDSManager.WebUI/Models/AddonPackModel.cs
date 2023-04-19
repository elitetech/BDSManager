namespace BDSManager.WebUI.Models;

public class AddonPackModel
{
    public string Path { get; set; } = string.Empty;
    public ManifestModel Manifest { get; set; } = new();
}