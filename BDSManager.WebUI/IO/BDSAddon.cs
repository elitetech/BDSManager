using System.IO.Compression;
using BDSManager.WebUI.Models;
using Newtonsoft.Json;

namespace BDSManager.WebUI.IO;

public class BDSAddon
{
    private readonly IConfiguration _configuration;
    private readonly ServerProperties _serverProperties;
    private readonly string _addonsPath = string.Empty;
    private readonly string _serversPath = string.Empty;
    private readonly List<AddonPackModel> _addonPacks = new();
    private readonly string[] _validExtensions = {"zip", "mcpack", "mcaddon"};

    public BDSAddon(IConfiguration configuration, ServerProperties serverProperties)
    {
        _configuration = configuration;
        _serverProperties = serverProperties;
        _serversPath = _configuration.GetChildren().FirstOrDefault(x => x.Key == "ServersPath")?.Value ?? string.Empty;
        _addonsPath = configuration.GetChildren().FirstOrDefault(x => x.Key == "AddonsPath")?.Value ?? string.Empty;
        SetupAvailableAddons();
    }

    private void SetupAvailableAddons()
    {
        if(!Directory.Exists(_addonsPath))
            Directory.CreateDirectory(_addonsPath);
        foreach(var directory in Directory.GetDirectories(_addonsPath))
        {
            var directoryName = Path.GetFileName(directory);
            if (directoryName == "addons")
                continue;

            var pack = new AddonPackModel
            {
                Path = directory,
                Manifest = JsonConvert.DeserializeObject<ManifestModel>(File.ReadAllText(Path.Combine(directory, "manifest.json")))
            };
            
            if(_addonPacks.Any(x => x.Path == pack.Path))
                continue;
            
            _addonPacks.Add(pack);
        }
    }

    public List<AddonPackModel> GetAvailableAddons()
    {
        return _addonPacks;
    }

    public void SaveAddon(FileStream file){
        if(!_validExtensions.Contains(Path.GetExtension(file.Name)))
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

        File.Copy(pack.Path, Path.Combine(destinationPath, Path.GetFileName(pack.Path)));
        
        if(server.Addons.FirstOrDefault(x => x.Manifest.header.uuid == pack.Manifest.header.uuid) is AddonPackModel addon)
            server.Addons.Remove(addon);

        server.Addons.Add(pack);

        if(isResourcePack)
            _serverProperties.SaveResourcePacks(server);
        else
            _serverProperties.SaveBehaviorPacks(server);
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

        if(isResourcePack)
            _serverProperties.SaveResourcePacks(server);
        else
            _serverProperties.SaveBehaviorPacks(server);
    }
}