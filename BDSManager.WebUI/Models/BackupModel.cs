namespace BDSManager.WebUI.Models;

public class BackupModel
{
    public bool BackupEnabled { get; set; } = false;
    public int BackupInterval { get; set; } = 24;
    public int BackupKeepCount { get; set; } = 5;
    public DateTime? NextBackup { get; set; }
}