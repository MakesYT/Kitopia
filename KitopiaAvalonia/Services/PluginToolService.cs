using System;
using Avalonia.Threading;
using Core.SDKs.Services;
using Microsoft.Extensions.DependencyInjection;

namespace KitopiaAvalonia.Services;

public class PluginToolService : IPluginToolService
{
    public void RequestUninstallPlugin(string pluginId)
    {
        Dispatcher.UIThread.InvokeAsync( () =>
        {
            try
            {
                ServiceManager.Services.GetService<MainWindow>()?.FrameView?.BackStack.Clear();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                
            }
        });
        
    }
}