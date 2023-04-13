using BDSManager.WebUI.Models;
using BDSManager.WebUI.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;

namespace BDSManager.WebUI.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly OptionsIO _optionsIO = new();

    public IndexModel(ILogger<IndexModel> logger)
    {
        _logger = logger;
    }

    public void OnGet()
    {
        ManagerOptionsModel options = _optionsIO.LoadOptions();
        if (_optionsIO.FirstSetup)
        {
            // redirect to new server page

        }
    }
}
