using System.Windows;
using PluginCore;

namespace PluginDemo;

public class Class1 : IPlugin
{
    public Class1()
    {
        MessageBox.Show("dd");
    }

    public static PluginInfo PluginInfo()
    {
        return new PluginInfo()
        {
            PluginName = "Demo",
            PluginId = "Demo",
            Author = "Kitopia",
            VersionInt = 0,
            Version = "1.0.0"
        };
    }

    public void OnEnabled()
    {
    }

    public void OnDisabled()
    {
    }
}