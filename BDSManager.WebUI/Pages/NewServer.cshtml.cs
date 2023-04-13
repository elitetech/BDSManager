using BDSManager.WebUI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

public class NewServerModel : PageModel
{
    private readonly ILogger<NewServerModel> _logger;
    public ServerOptionsModel Server { get; set; } = new();

    public NewServerModel(ILogger<NewServerModel> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public void OnGet()
    {

    }

    [HttpPost]
    public void New(NewServerModel model)
    {
        ServerModel server = new()
        {
            Path = model.Server.Path,
            Icon = model.Server.Icon,
            Version = model.Server.Version,
            Options = model.Server,
            AllowList = model.Server.AllowList,
            Permissions = model.Server.Permissions
        };

    }
}