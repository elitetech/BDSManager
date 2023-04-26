using System.IO.Compression;
using System.Text.RegularExpressions;
using AngleSharp;
using AngleSharp.Html.Parser;
using BDSManager.WebUI.Models;
using BDSManager.WebUI.Services;

namespace BDSManager.WebUI.IO;

public class BDSUpdater
{
    private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;
    private readonly ServerProperties _serverProperties;
    private readonly MinecraftServerService _minecraftServerService;
    private readonly string _url = "https://www.minecraft.net/en-us/download/server/bedrock/";
    private readonly string? _downloadPath;
    private readonly string? _serversPath;

    public BDSUpdater(Microsoft.Extensions.Configuration.IConfiguration configuration, ServerProperties serverProperties, MinecraftServerService minecraftServerService)
    {
        _configuration = configuration;
        _serverProperties = serverProperties;
        _minecraftServerService = minecraftServerService;
        _downloadPath = _configuration["DownloadPath"];
        _serversPath = _configuration["ServersPath"];
    }

    public async Task UpdateBedrockServerAsync(ServerModel server)
    {
        if(string.IsNullOrEmpty(_downloadPath))
            throw new Exception("Download path not set in configuration");
        
        if(string.IsNullOrEmpty(_serversPath))
            throw new Exception("Servers path not set in configuration");

        if(string.IsNullOrEmpty(server.Path))
            throw new Exception("Server path is not set");

        if(!Directory.Exists(_serversPath))
            Directory.CreateDirectory(_serversPath);
        if(!Directory.Exists(_downloadPath))
            Directory.CreateDirectory(_downloadPath);

        var destinationPath = Path.Combine(_serversPath, server.Path);

        if(!Directory.Exists(destinationPath))
            Directory.CreateDirectory(destinationPath);

        if(await IsInstalledBedrockServerCurrentAsync(server))
            return;
            

        var filePath = await IsDownloadedBedrockServerCurrentAsync() ? await GetDownloadedBedrockServerPathAsync() : await DownloadBedrockServerAsync();
        if(string.IsNullOrEmpty(filePath))
            throw new Exception("Could not download bedrock server");
        
        var instance = _minecraftServerService.ServerInstances.FirstOrDefault(x => x.Path == server.Path);
        if(instance?.ServerProcess != null && !instance.ServerProcess.HasExited)
            await _minecraftServerService.StopServerInstance(instance, "UPDATE");
        ZipFile.ExtractToDirectory(filePath, destinationPath, true);

        _serverProperties.SaveServerProperties(server);
        _serverProperties.SavePermissions(server);
        _serverProperties.SaveAllowList(server);
        _serverProperties.SavePlayers(server);
        _serverProperties.SaveResourcePacks(server);
        _serverProperties.SaveBehaviorPacks(server);
        _serverProperties.SaveBackupSettings(server);
        _serverProperties.SaveUpdateSettings(server);


        var version = ParseVersionFromFileName(filePath);
        File.WriteAllText(Path.Combine(destinationPath, "version.txt"), version);
        server.Version = version;
    }

    private async Task<string?> DownloadBedrockServerAsync()
    {
        string? downloadLink = await GetBedrockServerDownloadLinkAsync();
        if (downloadLink == null)
            throw new Exception("Could not find download link");

        string? version = ParseVersionFromFileName(downloadLink);
        if (version == null)
            throw new Exception("Could not parse version from download link");

        string? fileName = Path.GetFileName(downloadLink);
        if (fileName == null)
            throw new Exception("Could not parse file name from download link");

        if(string.IsNullOrEmpty(_downloadPath))
            throw new Exception("Download path not set in configuration");

        string? filePath = Path.Combine(_downloadPath, fileName);
        if (filePath == null)
            throw new Exception("Could not parse file path from download link");
        
        using var httpClient = new HttpClient();
        using var response = await httpClient.GetAsync(downloadLink);
        response.EnsureSuccessStatusCode();
        
        using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
        await response.Content.CopyToAsync(fileStream);
        return filePath;
    }

    private async Task<bool> IsDownloadedBedrockServerCurrentAsync()
    {

        string? currentVersion = GetDownloadedBedrockServerVersion();
        if (currentVersion == null)
            return false;

        string? downloadLink = await GetBedrockServerDownloadLinkAsync();
        if (downloadLink == null)
            return true;

        string? latestVersion = ParseVersionFromFileName(downloadLink);
        if (latestVersion == null)
            return true;

        if (latestVersion == currentVersion)
            return true;

        return false;
    }

    private async Task<bool> IsInstalledBedrockServerCurrentAsync(ServerModel server)
    {
        if(string.IsNullOrEmpty(_serversPath))
            throw new Exception("Servers path not set in configuration");

        if(string.IsNullOrEmpty(server.Path))
            throw new Exception("Server path is not set");

        var destinationPath = Path.Combine(_serversPath, server.Path);

        string? currentVersion = GetInstalledVersion(destinationPath);
        if (currentVersion == null)
            return false;

        string? downloadLink = await GetBedrockServerDownloadLinkAsync();
        if (downloadLink == null)
            return true;

        string? latestVersion = ParseVersionFromFileName(downloadLink);
        if (latestVersion == null)
            return true;

        if (latestVersion == currentVersion)
            return true;

        return false;
    }

    private string? GetDownloadedBedrockServerVersion()
    {

        if(string.IsNullOrEmpty(_downloadPath))
            throw new Exception("Download path not set in configuration");

        var files = Directory.GetFiles(_downloadPath, "bedrock-server-*.zip").ToList();
        if (files.Count == 0)
            return null;

        var fileNames = files
            .Select(x => Path.GetFileName(x))
            .OrderByDescending(x => x);

        return ParseVersionFromFileName(fileNames.First());
    }

    private Task<string> GetDownloadedBedrockServerPathAsync()
    {
        if(string.IsNullOrEmpty(_downloadPath))
            throw new Exception("Download path not set in configuration");

        var files = Directory.GetFiles(_downloadPath, "bedrock-server-*.zip").ToList();
        if (files.Count == 0)
            return Task.FromResult(string.Empty);

        var fileNames = files
            .Select(x => Path.GetFileName(x))
            .OrderByDescending(x => x);

        var path = Path.Combine(_downloadPath, fileNames.First());
        return Task.FromResult(path);
    }

    private async Task<string?> GetBedrockServerDownloadLinkAsync(string platform = "win", bool preview = false)
    {
        var config = Configuration.Default.WithDefaultLoader();
        var context = BrowsingContext.New(config);
        var document = await context.OpenAsync(_url);

        var previewBit = preview ? "-preview" : "";
        var downloadButton = document.QuerySelector($"a[href*='https://minecraft.azureedge.net/bin-{platform}{previewBit}/']");
        return downloadButton?.GetAttribute("href");
    }

    private string? ParseVersionFromFileName(string downloadLink)
    {
        var versionRegex = new Regex(@"bedrock-server-(\d+\.\d+\.\d+\.\d+)\.zip");
        var match = versionRegex.Match(downloadLink);

        if (match.Success)
            return match.Groups[1].Value;

        return null;
    }

    private string? GetInstalledVersion(string serverPath)
    {
        var versionFilePath = Path.Combine(serverPath, "version.txt");
        if (!File.Exists(versionFilePath))
            return null;

        return File.ReadAllText(versionFilePath);
    }
}