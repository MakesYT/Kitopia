﻿using System;
using Avalonia.Threading;
using Core.SDKs.Services;

namespace KitopiaAvalonia.Services;

public class PluginToolService : IPluginToolService
{
    public void RequestUninstallPlugin(string pluginId)
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            try
            {
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        });
    }
}