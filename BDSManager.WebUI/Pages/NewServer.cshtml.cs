using BDSManager.WebUI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BDSManager.WebUI.IO;

public class NewServerModel : PageModel
{
    [BindProperty]
    public ServerModel Server { get; set; } = new();
    private readonly ILogger<NewServerModel> _logger;
    private readonly IConfiguration _configuration;
    private readonly BDSUpdater _bdsUpdater;
    private readonly OptionsIO _optionsIO;
    private readonly string? _path;

    public NewServerModel(ILogger<NewServerModel> logger, IConfiguration configuration, BDSUpdater bdsUpdater, OptionsIO optionsIO)
    {
        _logger = logger;
        _configuration = configuration;
        _bdsUpdater = bdsUpdater;
        _optionsIO = optionsIO;
        _path = _configuration["ServersPath"];
    }

    public IActionResult OnGet()
    {
        return Page();
    }

    public async Task<IActionResult> OnPostNew()
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