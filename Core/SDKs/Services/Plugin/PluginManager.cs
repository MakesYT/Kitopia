using log4net;

namespace Core.SDKs.Services.Plugin;

public class PluginManager
{
    private static readonly ILog Log = LogManager.GetLogger(nameof(PluginManager));

    public static void Init()
    {
    }

    public static Dictionary<string, Plugin> EnablePlugin = new();
}