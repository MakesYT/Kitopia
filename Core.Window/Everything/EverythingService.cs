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
                var task = Task.Run<bool>(()=> Everything64.Everything_QueryW(true));
                task.Wait(TimeSpan.FromSeconds(1));

                return task.Result;
            }
            catch (Exception)
            {
                return false;
            }
        }
        else
        {
            // 32-bit
            try
            {
                var task = Task.Run<bool>(()=> Everything32.Everything_QueryW(true));
                task.Wait(TimeSpan.FromSeconds(1));

                return task.Result;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
    
}