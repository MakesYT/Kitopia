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
            
            try
            {
                return Task.Run(()=> Everything64.Everything_QueryW(true)).Wait(TimeSpan.FromSeconds(1));
            }
            catch (Exception)
            {
                return false;
            }
        }
        else
        {
            // 32-bit
            Everything32.Everything_SetMax(1);
            try
            {
                return Task.Run(()=> Everything32.Everything_QueryW(true)).Wait(TimeSpan.FromSeconds(1));
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
    
}