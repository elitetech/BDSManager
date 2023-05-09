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
    public List<string>? Arguments { get; set; }
    public List<string>? SetValues { get; set; }
    public List<string>? QueryValues { get; set; }
}
