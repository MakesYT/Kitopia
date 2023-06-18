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
            Description = "这仅仅是一个演示插件",
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