using PluginCore;

namespace Core.SDKs.Services;

public class PluginManager
{
    public void Init()
    {
    }

    public List<PluginInfo> ScannedPlugin = new();
    public List<PluginInfo> EnablePlugin = new();
}