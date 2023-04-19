using System.IO.Compression;
using BDSManager.WebUI.Models;
using Newtonsoft.Json;

namespace BDSManager.WebUI.IO;

public class BDSAddon
{
    private readonly string _addonsPath;
    private readonly List<AddonPackModel> _addonPacks = new();
    private readonly string[] _validExtensions = {"zip", "mcpack", "mcaddon"};

    public BDSAddon(string addonsPath)
    {
        _addonsPath = addonsPath;

    }

    private void SetupAvailableAddons()
    {
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

        var bpPath = Path.Combine(server.Path, "behavior_packs");
        var rpPath = Path.Combine(server.Path, "resource_packs");

        var isResourcePack = pack.Manifest.modules.Any(x => x.type == "resource");


            
    }
}