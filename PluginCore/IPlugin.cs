using System;

namespace PluginCore;

public interface IPlugin
{
    public static PluginInfo PluginInfo()
    {
        return new PluginInfo();
    }

    public void OnEnabled();
    public void OnDisabled();
    public static abstract IServiceProvider GetServiceProvider();
}