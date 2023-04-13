using System.IO.Compression;
using System.Text.RegularExpressions;
using AngleSharp;
using AngleSharp.Html.Parser;
using BDSManager.WebUI.Models;

namespace BDSManager.WebUI.IO;

public class BDSUpdater
{
    private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;
    private readonly ServerProperties _serverProperties;
    private readonly string _url = "https://www.minecraft.net/en-us/download/server/bedrock/";

    public BDSUpdater(Microsoft.Extensions.Configuration.IConfiguration configuration, ServerProperties serverProperties)
    {
        _configuration = configuration;
        _serverProperties = serverProperties;
    }

    public async Task UpdateBedrockServerAsync(ServerModel server)
    {
        if(_configuration["DownloadPath"] == null)
            throw new Exception("Download path not set in configuration");

        bool isCurrent = await IsDownloadedBedrockServerCurrentAsync();
        if (isCurrent == true)
            return;

        var filePath = await DownloadBedrockServerAsync();
        if(filePath == null)
            throw new Exception("Could not download bedrock server");

        ZipFile.ExtractToDirectory(filePath, server.Path);

        _serverProperties.SaveServerProperties(server);
        _serverProperties.SavePermissions(server);
        _serverProperties.SaveAllowList(server);

        File.WriteAllText(Path.Combine(server.Path, "version.txt"), ParseVersionFromFileName(filePath));
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

        string? filePath = Path.Combine(_configuration["DownloadPath"], fileName);
        if (filePath == null)
            throw new Exception("Could not parse file path from download link");
        
        using var httpClient = new HttpClient();
        using var response = await httpClient.GetAsync(downloadLink);
        response.EnsureSuccessStatusCode();
        
        using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
        await response.Content.CopyToAsync(fileStream);
        return filePath;
    }

    public async Task<bool> IsDownloadedBedrockServerCurrentAsync()
    {

        string? currentVersion = GetDownloadedBedrockServerVersion();
        if (currentVersion == null)
            return false;

        string? downloadLink = await GetBedrockServerDownloadLinkAsync();
        if (downloadLink == null)
            throw new Exception("Could not find download link");

        string? latestVersion = ParseVersionFromFileName(downloadLink);
        if (latestVersion == null)
            throw new Exception("Could not parse version from download link");

        if (latestVersion == currentVersion)
            return true;

        return false;
    }

    private string? GetDownloadedBedrockServerVersion()
    {
        var files = Directory.GetFiles(_configuration["DownloadPath"], "bedrock-server-*.zip").ToList();
        if (files.Count == 0)
            return null;

        var fileNames = files
            .Select(x => Path.GetFileName(x))
            .OrderByDescending(x => x);

        return ParseVersionFromFileName(fileNames.First());
    }

    public async Task<string?> GetBedrockServerDownloadLinkAsync(string platform = "win", bool preview = false)
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
}