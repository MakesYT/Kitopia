#region

using System;
using System.Windows;
using System.Windows.Controls;
using Core.SDKs.Services.Plugin;

#endregion

namespace Kitopia.View.Pages;

public partial class HomePage : Page
{
    static Core.SDKs.Services.Plugin.Plugin _plugin;

    public HomePage()
    {
        InitializeComponent();
    }

    private void Button_OnClick(object sender, RoutedEventArgs e)
    {
        _plugin = new Core.SDKs.Services.Plugin.Plugin(
            "D:\\WPF.net\\uToolkitopia\\uToolkitopia\\bin\\Debug\\net8.0-windows10.0.19041.0\\plugins\\KitopiaEx\\KitopiaEx.dll");
        PluginManager.EnablePlugin.Add("Kitopia_KitopiaEx", _plugin
        );
    }

    private void Button_OnClick2(object sender, RoutedEventArgs e)
    {
        _plugin = null;
        Core.SDKs.Services.Plugin.Plugin.UnloadByPluginInfo("Kitopia_KitopiaEx", out var weakReference);
        PluginManager.EnablePlugin.Remove("Kitopia_KitopiaEx");
        while (weakReference.IsAlive)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }

    private void Button_OnClick3(object sender, RoutedEventArgs e)
    {
        for (int i = 0; i < 10; i++)
        {
            GC.Collect(2, GCCollectionMode.Aggressive);
            GC.WaitForPendingFinalizers();
        }
    }
}