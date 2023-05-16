using System.IO.Compression;
using BDSManager.WebUI.Models;
using Newtonsoft.Json;
using BDSManager.WebUI.IO;

namespace BDSManager.WebUI.IO;

public class BDSAddon
{
    private readonly IConfiguration _configuration;
    private readonly ServerProperties _serverProperties;
    private readonly DirectoryIO _directoryIO;
    private readonly string _addonsPath = string.Empty;
    private readonly string _serversPath = string.Empty;
    private readonly List<AddonPackModel> _addonPacks = new();
    private readonly string[] _validExtensions = {".zip", ".mcpack", ".mcaddon"};

    public BDSAddon(IConfiguration configuration, ServerProperties serverProperties, DirectoryIO directoryIO)
    {
        _configuration = configuration;
        _serverProperties = serverProperties;
        _directoryIO = directoryIO;
        _serversPath = _configuration.GetChildren().FirstOrDefault(x => x.Key == "ServersPath")?.Value ?? string.Empty;
        _addonsPath = configuration.GetChildren().FirstOrDefault(x => x.Key == "AddonsPath")?.Value ?? string.Empty;
        SetupAvailableAddons();
    }

    private void SetupAvailableAddons()
    {
        if(!Directory.Exists(_addonsPath))
            Directory.CreateDirectory(_addonsPath);
        foreach(var addonRoot in Directory.GetDirectories(_addonsPath))
        {
            var addonRootName = Path.GetFileName(addonRoot);
            if (addonRootName == "addons")
                continue;

            var manifestPath = Path.Combine(_addonsPath, addonRootName, "manifest.json");
            foreach (var directory in Directory.GetDirectories(addonRoot))
            {
                if(!File.Exists(manifestPath))
                    manifestPath = Path.Combine(directory, "manifest.json");
                if(!File.Exists(manifestPath))
                    continue;

                var pack = new AddonPackModel
                {
                    Path = addonRootName,
                    Manifest = JsonConvert.DeserializeObject<ManifestModel>(File.ReadAllText(manifestPath))
                };
                
                if(_addonPacks.Any(x => x.Manifest.header.uuid == pack.Manifest.header.uuid))
                    continue;
                
                _addonPacks.Add(pack);
            }
        }
    }

    public List<AddonPackModel> GetAvailableAddons()
    {
        return _addonPacks;
    }

    public Task SaveAddon(string filePath){
        // get file stream from file path
        using var file = new FileStream(filePath, FileMode.Open, FileAccess.Read);

        var extension = Path.GetExtension(filePath);
        if(!_validExtensions.Contains(extension))
            throw new Exception("Invalid file type");

        if (!Directory.Exists(_addonsPath))
            Directory.CreateDirectory(_addonsPath);

        var addonName = Path.GetFileNameWithoutExtension(file.Name);
        var addonDir = Path.Combine(_addonsPath, addonName);
        if (Directory.Exists(addonDir))
            Directory.Delete(addonDir, true);
        Directory.CreateDirectory(addonDir);

        // extract zip file
        using var zip = new ZipArchive(file);
        zip.ExtractToDirectory(addonDir);

        SetupAvailableAddons();

        return Task.CompletedTask;
    }

    public void DeleteAddon(string addonPath)
    {
        if (!Directory.Exists(addonPath))
            throw new Exception("Addon does not exist");

        Directory.Delete(addonPath, true);
        SetupAvailableAddons();
    }

    public void InstallAddon(AddonPackModel pack, ServerModel server)
    {
        if (!Directory.Exists(Path.Combine(_addonsPath, pack.Path)))
            throw new Exception("Addon does not exist");

        if (!Directory.Exists(Path.Combine(_serversPath, server.Path)))
            throw new Exception("Server does not exist");

        var bpPath = Path.Combine(_serversPath, server.Path, "behavior_packs");
        var rpPath = Path.Combine(_serversPath, server.Path, "resource_packs");

        var isResourcePack = pack.Manifest.modules.Any(x => x.type == "resources");

        var destinationPath = isResourcePack ? rpPath : bpPath;
        destinationPath = Path.Combine(destinationPath, pack.Path);

        if (!Directory.Exists(destinationPath))
            Directory.CreateDirectory(destinationPath);

        if(Directory.Exists(destinationPath))
            Directory.Delete(destinationPath, true);

        _directoryIO.Copy(Path.Combine(_addonsPath, pack.Path), destinationPath, true);
        
        if(server.Addons.FirstOrDefault(x => x.Manifest.header.uuid == pack.Manifest.header.uuid) is AddonPackModel addon)
            server.Addons.Remove(addon);

        server.Addons.Add(pack);

        _serverProperties.SaveServerSettings(server);
    }

    public void UninstallAddon(AddonPackModel pack, ServerModel server)
    {
        if (!Directory.Exists(pack.Path))
            throw new Exception("Addon does not exist");

        if (!Directory.Exists(server.Path))
            throw new Exception("Server does not exist");

        var bpPath = Path.Combine(_serversPath, server.Path, "behavior_packs");
        var rpPath = Path.Combine(_serversPath, server.Path, "resource_packs");

        var isResourcePack = pack.Manifest.modules.Any(x => x.type == "resource");

        var destinationPath = isResourcePack ? rpPath : bpPath;

        if (!Directory.Exists(destinationPath))
            Directory.CreateDirectory(destinationPath);

        if(Directory.Exists(Path.Combine(destinationPath, Path.GetFileName(pack.Path))))
            Directory.Delete(Path.Combine(destinationPath, Path.GetFileName(pack.Path)), true);

        if(server.Addons.FirstOrDefault(x => x.Manifest.header.uuid == pack.Manifest.header.uuid) is AddonPackModel addon)
            server.Addons.Remove(addon);

        _serverProperties.SaveServerSettings(server);
    }
}