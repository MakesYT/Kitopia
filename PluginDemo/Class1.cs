using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using PluginCore;

namespace PluginDemo;

public class Class1 : IPlugin
{
    public Class1()
    {
        MessageBox.Show("dd");
    }

    public static PluginInfo PluginInfo = new PluginInfo()
    {
        PluginName = "Demo",
        PluginId = "Demo",
        Description = "这仅仅是一个演示插件",
        Author = "Kitopia",
        VersionInt = 0,
        Version = "1.0.0"
    };

    public void OnEnabled()
    {
        MessageBox.Show("OnEnabled");
    }

    public void OnDisabled()
    {
    }

    public static IServiceProvider GetServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddSingleton<Class1>();
        services.AddSingleton<Test1>();
        return services.BuildServiceProvider();
    }
}