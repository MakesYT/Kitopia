﻿using System;
using Microsoft.Extensions.DependencyInjection;
using PluginCore;

namespace KitopiaEx;

public class KitopiaEx : IPlugin
{
    public static IServiceProvider ServiceProvider;

    private IPlugin _pluginImplementation;

    public void OnEnabled(IServiceProvider serviceProvider)
    {
        //MessageBox.Show("OnEnabled");
        Kitopia._i18n.TryAdd("System.Windows.Media.Imaging.BitmapSource", "图像BitmapSource");
        ServiceProvider = serviceProvider;
    }

    public void OnDisabled()
    {
    }

    public static IServiceProvider GetServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddSingleton<KitopiaEx>();
        services.AddSingleton<SearchItemEx>();
        services.AddSingleton<ClipboardEx>();
        services.AddSingleton<ImageTools>();
        services.AddSingleton<Test1>();
        services.AddSingleton<NodeInputConnector1>();
        services.AddSingleton<KeyboardSimulation>();
        return services.BuildServiceProvider();
    }
}