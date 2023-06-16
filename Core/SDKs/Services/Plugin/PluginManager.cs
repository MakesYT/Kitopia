using log4net;

namespace Core.SDKs.Services.Plugin;

public class PluginManager
{
    private static readonly ILog Log = LogManager.GetLogger(nameof(PluginManager));

    public void Init()
    {
        DirectoryInfo pluginsDirectoryInfo = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory + "plugins");
        if (!pluginsDirectoryInfo.Exists)
        {
            Log.Debug($"插件目录不存在创建{pluginsDirectoryInfo.FullName}");
            pluginsDirectoryInfo.Create();
        }

        foreach (FileInfo enumerateFile in pluginsDirectoryInfo.EnumerateFiles())
        {
            if (enumerateFile.Extension.Equals(".dll"))
            {
                ScannedPlugin.Add(new Plugin(enumerateFile.FullName));
            }
        }

        foreach (var plugin in ScannedPlugin)
        {
            if (EnablePlugin.Exists((e) =>
                {
                    if (e._pluginInfo.Equals(plugin._pluginInfo))
                    {
                        return true;
                    }

                    return false;
                }))
            {
                EnablePlugin.Add(plugin);
            }
        }
    }

    public static List<Plugin> ScannedPlugin = new();
    public static List<Plugin> EnablePlugin = new();
}