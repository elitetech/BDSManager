using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using BDSManager.WebUI.Models;

namespace BDSManager.WebUI.Services;

public class ServerInstance
{
    public string? Path { get; set; }
    public Process? ServerProcess { get; set; }
    public LinkedList<string> ConsoleOutput { get; set; } = new();
    public bool SaveQuery { get; set; } = false;
    public bool SaveCanResume { get; set; } = false;
}