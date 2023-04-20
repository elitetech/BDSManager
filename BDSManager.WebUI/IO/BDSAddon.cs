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
    private readonly string[] _validExtensions = {".zip", ".mcpack", ".mcaddon"};

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
        foreach(var addonRoot in Directory.GetDirectories(_addonsPath))
        {
            var addonRootName = Path.GetFileName(addonRoot);
            if (addonRootName == "addons")
                continue;

            foreach (var directory in Directory.GetDirectories(addonRoot))
            {
                var manifestPath = Path.Combine(directory, "manifest.json");
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

        var isResourcePack = pack.Manifest.modules.Any(x => x.type == "resource");

        var destinationPath = isResourcePack ? rpPath : bpPath;
        destinationPath = Path.Combine(destinationPath, pack.Path);

        if (!Directory.Exists(destinationPath))
            Directory.CreateDirectory(destinationPath);

        if(Directory.Exists(destinationPath))
            Directory.Delete(destinationPath, true);

        DirectoryCopy(pack.Path, destinationPath, true);
        
        if(server.Addons.FirstOrDefault(x => x.Manifest.header.uuid == pack.Manifest.header.uuid) is AddonPackModel addon)
            server.Addons.Remove(addon);

        server.Addons.Add(pack);

        if(isResourcePack)
            _serverProperties.SaveResourcePacks(server);
        else
            _serverProperties.SaveBehaviorPacks(server);
    }

    private void DirectoryCopy(string source, string destination, bool recursive)
    {
        DirectoryInfo dir = new DirectoryInfo(Path.Combine(_addonsPath, source));
        DirectoryInfo[] dirs = dir.GetDirectories();

        // If the source directory does not exist, throw an exception.
        if (!dir.Exists)
        {
            throw new DirectoryNotFoundException(
                "Source directory does not exist or could not be found: "
                + dir.FullName);
        }

        // If the destination directory does not exist, create it.
        if (!Directory.Exists(destination))
        {
            Directory.CreateDirectory(destination);
        }

        // Get the file contents of the directory to copy.
        FileInfo[] files = dir.GetFiles();

        foreach (FileInfo file in files)
        {
            // Create the path to the new copy of the file.
            string temppath = Path.Combine(destination, file.Name);

            // Copy the file.
            file.CopyTo(temppath, false);
        }

        // If copying subdirectories, copy them and their contents to new location.
        if (recursive)
        {
            foreach (DirectoryInfo subdir in dirs)
            {
                // Create the subdirectory.
                string temppath = Path.Combine(destination, subdir.Name);

                // Copy the subdirectories.
                DirectoryCopy(subdir.FullName, temppath, recursive);
            }
        }
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