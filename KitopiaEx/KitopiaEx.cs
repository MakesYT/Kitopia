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

    public static IServiceProvider ServiceProvider;

    private IPlugin _pluginImplementation;

    public void OnEnabled(IServiceProvider serviceProvider)
    {
        //MessageBox.Show("OnEnabled");
        Kitopia._i18n.TryAdd("System.Windows.Media.Imaging.BitmapSource", "图像BitmapSource");
        ServiceProvider = serviceProvider;
        serviceProvider.GetService<OcrEx>()!.InitOcr();
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
        services.AddSingleton<OcrEx>();
        services.AddSingleton<ImageTools>();
        services.AddSingleton<Test1>();
        return services.BuildServiceProvider();
    }
}