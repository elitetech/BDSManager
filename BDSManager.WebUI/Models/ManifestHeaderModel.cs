namespace BDSManager.WebUI.Models
{
    public class ManifestHeaderModel
    {
        public string? name { get; set; }
        public string? description { get; set; }
        public string? uuid { get; set; }
        public int[]? version { get; set; }
        public int[]? min_engine_version { get; set; }
    }
}