using System.Diagnostics;

namespace BDSManager.WebUI.Services;

public class MinecraftServerService
{
    private readonly Process _serverProcess;

    public MinecraftServerService()
    {
        _serverProcess = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "path/to/bedrock_server",
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        _serverProcess.OutputDataReceived += ServerProcess_OutputDataReceived;
        //_serverProcess.Start();
        //_serverProcess.BeginOutputReadLine();
    }

    private void ServerProcess_OutputDataReceived(object sender, DataReceivedEventArgs e)
    {
        // Handle server output
    }

    public void SendCommand(string command)
    {
        _serverProcess.StandardInput.WriteLine(command);
    }
}