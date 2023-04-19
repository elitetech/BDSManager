using BDSManager.WebUI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BDSManager.WebUI.IO;

namespace BDSManager.WebUI.Pages;
public class ManageServerModel : PageModel
{
    [BindProperty]
    public ServerModel Server { get; set; } = new();
    [BindProperty]
    public string HeaderString { get; set; } = "New Server";
    [BindProperty]
    public bool CreateNew { get; set; } = false;
    private readonly ILogger<ManageServerModel> _logger;
    private readonly IConfiguration _configuration;
    private readonly BDSUpdater _bdsUpdater;
    private readonly OptionsIO _optionsIO;
    private readonly ServerProperties _serverProperties;
    private readonly BDSAddon _bdsAddon;
    private readonly string? _path;

    public ManageServerModel(
        ILogger<ManageServerModel> logger, 
        IConfiguration configuration, 
        BDSUpdater bdsUpdater, 
        OptionsIO optionsIO, 
        ServerProperties serverProperties,
        BDSAddon bdsAddon
        )
    {
        _logger = logger;
        _configuration = configuration;
        _bdsUpdater = bdsUpdater;
        _optionsIO = optionsIO;
        _serverProperties = serverProperties;
        _bdsAddon = bdsAddon;
        _path = _configuration["ServersPath"];
    }

    public IActionResult OnGet(bool createNew = false, string? path = null)
    {
        HeaderString = createNew ? "New Server" : "Edit Server";
        CreateNew = createNew;
        if (!createNew && !string.IsNullOrEmpty(path))
        {
            if(!_optionsIO.ManagerOptions.Servers.Any(x => x.Path == path))
                return RedirectToPage("./Index");
            Server = _optionsIO.ManagerOptions.Servers.FirstOrDefault(x => x.Path == path) ?? new();
            
            return Page();
        }
        return Page();
    }
    public async Task<IActionResult> OnPostNewServer()
    {
        ServerModel server = new()
        {
            Options = Server.Options
        };

        if (string.IsNullOrEmpty(server.Path))
            server.Path = GetNextServerPath();

        await _bdsUpdater.UpdateBedrockServerAsync(server);
        _optionsIO.AddServer(server);
        return RedirectToPage("./Index");
    }

    public Task<IActionResult> OnPostSaveServer()
    {
        if (string.IsNullOrEmpty(Server.Path))
            return Task.FromResult<IActionResult>(RedirectToPage("./Index"));

        var server = _optionsIO.ManagerOptions.Servers.FirstOrDefault(x => x.Path == Server.Path);
        if (server == null)
            return Task.FromResult<IActionResult>(RedirectToPage("./Index"));

        _serverProperties.SaveServerProperties(Server);
        _serverProperties.SavePermissions(Server);
        _serverProperties.SaveAllowList(Server);
        _serverProperties.SavePlayers(Server);

        foreach (var addon in Server.Addons)
        {
            if(server.Addons.Any(x => x.Manifest.header.uuid == addon.Manifest.header.uuid))
                continue;
            _bdsAddon.InstallAddon(addon, Server);
        }

        foreach (var addon in server.Addons)
        {
            if(Server.Addons.Any(x => x.Manifest.header.uuid == addon.Manifest.header.uuid))
                continue;
            _bdsAddon.UninstallAddon(addon, Server);
        }

        _optionsIO.RefreshServers();
        return Task.FromResult<IActionResult>(RedirectToPage("./Index"));
    }

    private string GetNextServerPath()
    {
        if(_path == null)
            throw new Exception("ServersPath is not set in appsettings.json");
        
        if(!Directory.Exists(_path))
            Directory.CreateDirectory(_path);

        var dirs = Directory.GetDirectories(_path).ToList().OrderByDescending(x => x);
        var lastDir = dirs.FirstOrDefault();
        if(lastDir == null)
            return "00";
        else
            return (int.Parse(lastDir.Substring(lastDir.Length - 2)) + 1).ToString("00");
    }
}