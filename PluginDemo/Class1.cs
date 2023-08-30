#region

using System;
using Microsoft.Extensions.DependencyInjection;
using PluginCore;

#endregion

namespace PluginDemo;

public class Class1 : IPlugin
{
    public Class1()
    {
        //MessageBox.Show("dd");
    }

    public static PluginInfo PluginInfo = new()
    {
        PluginName = "Demo",
        PluginId = "Demo",
        Description = "这仅仅是一个演示插件",
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
        services.AddSingleton<Class1>();
        services.AddSingleton<Test1>();
        services.AddSingleton<Test>();
        return services.BuildServiceProvider();
    }
}