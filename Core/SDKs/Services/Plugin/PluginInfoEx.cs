using System.Drawing;
using PluginCore;

namespace Core.SDKs.Services.Plugin;

public class PluginInfoEx
{
    public string PluginName
    {
        set;
        get;
    }

    public string PluginId
    {
        set;
        get;
    }

    public string Description
    {
        set;
        get;
    }

    public string Author
    {
        set;
        get;
    }

    public int VersionInt
    {
        set;
        get;
    }

    public string Version
    {
        set;
        get;
    }

    public bool IsEnabled
    {
        set;
        get;
    }

    public string Error
    {
        set;
        get;
    }

    public string Path
    {
        set;
        get;
    }

    public Icon Icon
    {
        set;
        get;
    }

    public string ToPlgString()
    {
        return $"{Author}_{PluginId}";
    }

    public PluginInfo ToPluginInfo()
    {
        return new PluginInfo()
        {
            Author = Author,
            Description = Description,
            PluginId = PluginId,
            PluginName = PluginName,
            Version = Version,
            VersionInt = VersionInt
        };
    }
}