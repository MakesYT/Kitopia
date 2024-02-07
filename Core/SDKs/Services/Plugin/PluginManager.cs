#region

using Core.SDKs.CustomScenario;
using Core.SDKs.Services.Config;
using log4net;
using PluginCore;

#endregion

namespace Core.SDKs.Services.Plugin;

public class PluginManager
{
    private static readonly ILog Log = LogManager.GetLogger(nameof(PluginManager));

    public static Dictionary<string, Plugin> EnablePlugin = new();

    public static void Init()
    {
        Kitopia.ISearchItemTool = (ISearchItemTool)ServiceManager.Services.GetService(typeof(ISearchItemTool))!;
        Kitopia.IToastService = (IToastService)ServiceManager.Services.GetService(typeof(IToastService))!;
        Kitopia._i18n = CustomScenarioGloble._i18n;
        var pluginsDirectoryInfo = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory + "plugins");
        if (!pluginsDirectoryInfo.Exists)
        {
            Log.Debug($"插件目录不存在创建{pluginsDirectoryInfo.FullName}");
            pluginsDirectoryInfo.Create();
        }

        foreach (var directoryInfo in pluginsDirectoryInfo.EnumerateDirectories())
        {
            if (File.Exists($"{directoryInfo.FullName}\\{directoryInfo.Name}.dll"))
            {
                Log.Debug($"加载插件:{directoryInfo.Name}.dll");

                var pluginInfoEx = PluginInfoTool.GetPluginInfoEx($"{directoryInfo.FullName}\\{directoryInfo.Name}.dll",
                    out var alcWeakRef);


                while (alcWeakRef.IsAlive)
                {
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                }

                if (pluginInfoEx.Version != "error")
                {
                    if (ConfigManger.Config.EnabledPluginInfos.Any(e =>
                            e.PluginId == pluginInfoEx.PluginId && e.Author == pluginInfoEx.Author &&
                            e.VersionInt == pluginInfoEx.VersionInt))
                    {
                        Task.Run(() =>
                        {
                            Plugin.Load(pluginInfoEx);
                        }).Wait();
                    }
                }
            }
        }
    }
}