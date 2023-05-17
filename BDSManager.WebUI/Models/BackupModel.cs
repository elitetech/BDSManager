namespace BDSManager.WebUI.Models;

public class BackupModel
{
    public bool BackupEnabled { get; set; } = false;
    public bool WorldBackupEnabled { get; set; } = false;
    public int BackupInterval { get; set; } = 24;
    public int WorldBackupInterval { get; set; } = 1;
    public int BackupKeepCount { get; set; } = 5;
    public int WorldBackupKeepCount { get; set; } = 24;
    public DateTime? NextBackup { get; set; }
    public DateTime? NextWorldBackup { get; set; }
    public DateTime? LastBackup { get; set; }
    public DateTime? LastWorldBackup { get; set; }
}