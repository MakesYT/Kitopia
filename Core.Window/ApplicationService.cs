using System.IO;
using Avalonia;
using Core.SDKs.Services;
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
}