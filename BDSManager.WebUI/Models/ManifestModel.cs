namespace BDSManager.WebUI.Models;

public class ManifestModel
{
    public int format_version { get; set; } = 2;
    public ManifestHeaderModel header { get; set; } = new();
    public List<ManifestModuleModel> modules { get; set; } = new();
    public List<ManifestDependenciesModel>? dependencies { get; set; } = new();
    
}