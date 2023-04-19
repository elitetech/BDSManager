using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace BDSManager.WebUI.Pages;

public class RemoveServer : PageModel
{
    private readonly ILogger<RemoveServer> _logger;

    public RemoveServer(ILogger<RemoveServer> logger)
    {
        _logger = logger;
    }

    public void OnGet()
    {
    }
}