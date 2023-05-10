using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BDSManager.WebUI.Models;

public class MinecraftTimeCommandModel : IMinecraftCommand
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public String? Usage { get; set; }
    public List<string> Arguments { get; set; } = new ();
    public List<string> SetValues { get; set; } = new ();
    public List<string> QueryValues { get; set; } = new ();
}
