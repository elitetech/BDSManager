namespace BDSManager.WebUI.Models;

public class ServerOptionsModel
{
    public string Name { get; set; } = "Minecraft Server";
    public string Port { get; set; } = "19132";
    public string Portv6 { get; set; } = "19133";
    public string MaxPlayers { get; set; } = "10";
    public string Gamemode { get; set; } = "survival";
    public string Difficulty { get; set; } = "easy";
    public string AllowCheats { get; set; } = "false";
    public string OnlineMode { get; set; } = "true";
    public string AllowList { get; set; } = "false";
    public string MaxThreads { get; set; } = "8";
    public string ViewDistance { get; set; } = "32";
    public string TickDistance { get; set; } = "4";
    public string PlayerIdleTimeout { get; set; } = "30";
    public string LevelName { get; set; } = "Bedrock level";
    public string LevelSeed { get; set; } = "";
    public string CompressionThreshold { get; set; } = "1";
    public string DefaultPlayerPermissionLevel { get; set; } = "member";
    public string TexturePackRequired { get; set; } = "false";
    public string ContentLog { get; set; } = "false";
    public string ForceGamemode { get; set; }  = "false";
    public string ServerAuthoritativeMovement { get; set; } = "server-auth";
    public string PlayerMovementScoreThreshold { get; set; } = "20";
    public string PlayerMovementDistanceThreshold { get; set; } = "0.3";
    public string PlayerMovementDurationThresholdInMs { get; set; } = "500";
    public string PlayerMovementActionDirectionThreshold { get; set; } = "0.85";
    public string CorrectPlayerMovement { get; set; } = "false";
    public string ServerAuthoritativeBlockBreaking { get; set; } = "false";
}