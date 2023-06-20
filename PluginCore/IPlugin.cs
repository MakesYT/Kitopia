using System;

namespace PluginCore;

public interface IPlugin
{
    public void OnEnabled();
    public void OnDisabled();
    public static abstract IServiceProvider GetServiceProvider();
}