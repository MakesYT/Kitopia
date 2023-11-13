#region

using System;

#endregion

namespace PluginCore;

public interface IPlugin
{
    public void OnEnabled(IServiceProvider serviceProvider);
    public void OnDisabled();

    public static abstract IServiceProvider GetServiceProvider();
}