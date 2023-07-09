using Core.SDKs.Services.Config;
using log4net;

namespace Core.SDKs.Services.Plugin;

public class PluginManager
{
    private static readonly ILog Log = LogManager.GetLogger(nameof(PluginManager));

    public static void Init()
    {
        ThreadPool.QueueUserWorkItem((e) =>
        {
            DirectoryInfo pluginsDirectoryInfo = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory + "plugins");
            if (!pluginsDirectoryInfo.Exists)
            {
                Log.Debug($"插件目录不存在创建{pluginsDirectoryInfo.FullName}");
                pluginsDirectoryInfo.Create();
            }

            foreach (DirectoryInfo directoryInfo in pluginsDirectoryInfo.EnumerateDirectories())
            {
                if (File.Exists($"{directoryInfo.FullName}\\{directoryInfo.Name}.dll"))
                {
                    Log.Debug($"加载插件:{directoryInfo.Name}.dll");
                    var pluginInfoEx = Plugin.GetPluginInfoEx($"{directoryInfo.FullName}\\{directoryInfo.Name}.dll",
                        out var alcWeakRef);


                    while (alcWeakRef.IsAlive)
                    {
                        GC.Collect();
                        GC.WaitForPendingFinalizers();
                    }

                    if (pluginInfoEx.Version != "error")
                    {
                        if (ConfigManger.Config.EnabledPluginInfos.Contains(pluginInfoEx.ToPluginInfo()))
                        {
                            PluginManager.EnablePlugin.Add($"{pluginInfoEx.Author}_{pluginInfoEx.PluginId}",
                                new Plugin(pluginInfoEx.Path));
                        }
                    }
                }
            }
        });
    }

    public static Dictionary<string, Plugin> EnablePlugin = new();
}