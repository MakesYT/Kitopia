using System;
using Microsoft.Extensions.DependencyInjection;
using PluginCore;

namespace KitopiaEx;

public class KitopiaEx : IPlugin
{
    public static PluginInfo PluginInfo = new()
    {
        PluginName = "Kitopia拓展",
        PluginId = "KitopiaEx",
        Description = "Kitopia拓展",
        Author = "Kitopia",
        VersionInt = 1,
        Version = "1.0.0"
    };

    public void OnEnabled()
    {
        //MessageBox.Show("OnEnabled");
    }

    public void OnDisabled()
    {
    }

    public static IServiceProvider GetServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddSingleton<KitopiaEx>();
        services.AddSingleton<SearchItemEx>();
        return services.BuildServiceProvider();
    }
}