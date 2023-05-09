using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BDSManager.WebUI.Models;

public class MinecraftEnchantmentModel
{
    public string? Name { get; set; }
    public string? Image { get; set; }
    public string? ID { get; set; }
    public string? Description { get; set; }
    public int MaxLevel { get; set; }
    public List<string>? ItemTypes { get; set; }
}
