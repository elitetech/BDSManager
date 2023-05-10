using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BDSManager.WebUI.Models;

public class MinecraftTeleportCommandModel : IMinecraftCommand
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public String? Usage { get; set; }
}
