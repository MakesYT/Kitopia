using Core.SDKs.Services.Config;
using log4net;
using PluginCore;

namespace Core.SDKs.Services.Plugin;

public static class PluginInfoTool
{
    private static readonly ILog Log = LogManager.GetLogger(nameof(PluginInfoTool));

    public static PluginInfo GetPluginInfoEx(string assemblyPath, out WeakReference alcWeakRef)
    {
        var firstOrDefault = PluginManager.EnablePlugin.FirstOrDefault(e => e.Value._dll!.Location == assemblyPath)
            .Value;
        if (firstOrDefault is not null)
        {
            alcWeakRef = new WeakReference(null);
            return firstOrDefault.PluginInfo;
        }

        Log.Debug($"加载插件Info:{assemblyPath}");
        var alc = new AssemblyLoadContextH(assemblyPath, $"{assemblyPath}_pluginInfo");

        // Create a weak reference to the AssemblyLoadContext that will allow us to detect
        // when to unload completes.
        alcWeakRef = new WeakReference(alc);

        // Load the plugin assembly into the HostAssemblyLoadContext.
        // NOTE: the assemblyPath must be an absolute path.
        var a = alc.LoadFromAssemblyPath(assemblyPath);

        // Get the plugin interface by calling the PluginClass.GetInterface method via reflection.
        var t = a.GetExportedTypes();
        var pluginInfoEx = new PluginInfo() { Version = "error" };
        foreach (var type in t)
        {
            if (type.GetInterface("IPlugin") != null)
            {
                var pluginInfo = (PluginInfo)type.GetField("PluginInfo").GetValue(null);

                if (ConfigManger.Config.EnabledPluginInfos.Contains(pluginInfo))
                {
                    pluginInfoEx = pluginInfo;
                    break;
                }
                else if (ConfigManger.Config.EnabledPluginInfos.Exists(e =>
                         {
                             if (e.PluginName != pluginInfo.PluginName)
                             {
                                 return false;
                             }

                             if (e.Author != pluginInfo.Author)
                             {
                                 return false;
                             }

                             if (e.VersionInt != pluginInfo.VersionInt)
                             {
                                 return true;
                             }

                             return false;
                         })) //有这个插件但是版本不对
                {
                    pluginInfoEx = pluginInfo;
                    break;
                }
                else
                {
                    pluginInfoEx = pluginInfo;
                    break;
                }
            }
        }

        //Console.WriteLine($"Response from the plugin: GetVersion(): {pluginInfoEx.PluginId}");


        // This initiates the unload of the HostAssemblyLoadContext. The actual unloading doesn't happen
        // right away, GC has to kick in later to collect all the stuff.
        alc.Unload();
        pluginInfoEx.Path = assemblyPath;
        return pluginInfoEx;
    }
}