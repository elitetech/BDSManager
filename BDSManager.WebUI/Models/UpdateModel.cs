namespace BDSManager.WebUI.Models
{
    public class UpdateModel
    {
        public bool UpdateEnabled { get; set; } = false;
        public int UpdateInterval { get; set; } = 24;
        public DateTime? NextUpdate { get; set; }
    }
}