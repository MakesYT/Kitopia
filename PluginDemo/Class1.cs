using PluginCore;

namespace PluginDemo;

public class Class1 : IPlugin
{
    public PluginInfo PluginInfo()
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