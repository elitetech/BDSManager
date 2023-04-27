using BDSManager.WebUI.Models;
using BDSManager.WebUI.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using BDSManager.WebUI.Services;

namespace BDSManager.WebUI.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly OptionsIO _optionsIO;
    private readonly MinecraftServerService _minecraftServerService;
    [BindProperty]
    public List<ServerModel> Servers { get; set; } = new();
    [BindProperty]
    public List<ItemModel> Items { get; set; } = new();
    [BindProperty]
    public List<ServerInstance> ServerInstances { get; set; } = new();

    public IndexModel(ILogger<IndexModel> logger, MinecraftServerService minecraftServerService, OptionsIO optionsIO)
    {
        _logger = logger;
        _minecraftServerService = minecraftServerService;
        _optionsIO = optionsIO;
        Servers = _optionsIO.ManagerOptions.Servers;
        ServerInstances = _minecraftServerService.ServerInstances;
        Items = _optionsIO.ManagerOptions.Items;
    }

    public IActionResult OnGet()
    {
        
        if (_optionsIO.FirstSetup)
            return RedirectToPage("./ManageServer", new { createNew = true });
        
        return Page();
    }
}
