#region

using System.IO;
using System.Windows;
using Core.SDKs.Services.Config;
using Core.SDKs.Tools;
using log4net;
using PluginCore;

#endregion

namespace Core.SDKs.Services.Plugin;

public class PluginManager
{
    private static readonly ILog Log = LogManager.GetLogger(nameof(PluginManager));
    public static bool isInitialized = false;

    public static Dictionary<string, Plugin> EnablePlugin = new();

    public static void Init()
    {
        ThreadPool.QueueUserWorkItem((e) =>
        {
            Kitopia.ISearchItemTool = (ISearchItemTool)ServiceManager.Services.GetService(typeof(ISearchItemTool))!;
            Kitopia.IToastService = (IToastService)ServiceManager.Services.GetService(typeof(IToastService))!;
            Kitopia._i18n = BaseNodeMethodsGen._i18n;
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
                            Application.Current.Dispatcher.BeginInvoke(() =>
                            {
                                Plugin.LoadBypath($"{pluginInfoEx.Author}_{pluginInfoEx.PluginId}", pluginInfoEx.Path);
                            }).Wait();
                        }
                    }
                }
            }

#if DEBUG
            Log.Debug("Debug加载测试插件");

            var pluginInfoEx1 = Plugin.GetPluginInfoEx(
                @"D:\WPF.net\uToolkitopia\KitopiaEx\bin\Debug\net8.0-windows10.0.19041.0\KitopiaEx.dll",
                out var alcWeakRef1);


            while (alcWeakRef1.IsAlive)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }

            Plugin.LoadBypath($"{pluginInfoEx1.Author}_{pluginInfoEx1.PluginId}", pluginInfoEx1.Path);
            //((ITaskEditorOpenService)ServiceManager.Services!.GetService(typeof(ITaskEditorOpenService))!)!.Open();


#endif
            isInitialized = true;
        });
    }
}