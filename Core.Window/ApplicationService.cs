using System.IO;
using Avalonia;
using Core.SDKs.Services;
using Core.SDKs.Services.Config;
using Vanara.PInvoke;

namespace Core.Window;

public class ApplicationService : IApplicationService
{
    public void Restart()
    {
       
        Shell32.ShellExecute(IntPtr.Zero, "open", AppDomain.CurrentDomain.FriendlyName+".exe", "", AppDomain.CurrentDomain.BaseDirectory,
            ShowWindowCommand.SW_NORMAL);
        System.Environment.Exit(0);
    }

    public void Stop()
    {
        ConfigManger.Save();
        Environment.Exit(0);
    }
}