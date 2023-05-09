using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BDSManager.WebUI.Models;

public class MinecraftItemModel
{
    public string? Name { get; set; }
    public string? Image { get; set; }
    public string? IDName { get; set; }
    public int ID { get; set; }
    public int DataValue { get; set; }
}
