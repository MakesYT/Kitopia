using Core.SDKs.Services;
using PluginCore;

namespace Core.Window.Everything;

public class EverythingService : IEverythingService
{
    public bool isRun()
    {
        if (IntPtr.Size == 8)
        {
            // 64-bit
            Everything64.Everything_SetMax(1);
            return Everything64.Everything_QueryW(true);
        }
        else
        {
            // 32-bit
            Everything32.Everything_SetMax(1);
            return Everything32.Everything_QueryW(true);
        }
    }
    
}