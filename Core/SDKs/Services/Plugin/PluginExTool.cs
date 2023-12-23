using Core.SDKs.Services.Config;
using PluginCore;

namespace Core.SDKs.Services.Plugin;

public static class PluginExTool
{
    public static PluginInfo GetPluginInfoEx(string assemblyPath, out WeakReference alcWeakRef)
    {
        var alc = new AssemblyLoadContextH(assemblyPath, "pluginInfo");

        // Create a weak reference to the AssemblyLoadContext that will allow us to detect
        // when the unload completes.
        alcWeakRef = new WeakReference(alc);

        // Load the plugin assembly into the HostAssemblyLoadContext.
        // NOTE: the assemblyPath must be an absolute path.
        var a = alc.Assembly;

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
                    pluginInfoEx = new PluginInfo()
                    {
                        Author = pluginInfo.Author,
                        Error = "",
                        IsEnabled = false,
                        Path = assemblyPath,
                        PluginId = pluginInfo.PluginId,
                        PluginName = pluginInfo.PluginName,
                        Description = pluginInfo.Description,
                        Version = pluginInfo.Version,
                        VersionInt = pluginInfo.VersionInt
                    };
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
                    pluginInfoEx = new PluginInfo()
                    {
                        Author = pluginInfo.Author,
                        Error = "插件版本不一致",
                        IsEnabled = false,
                        Path = assemblyPath,
                        PluginId = pluginInfo.PluginId,
                        PluginName = pluginInfo.PluginName,
                        Description = pluginInfo.Description,
                        Version = pluginInfo.Version,
                        VersionInt = pluginInfo.VersionInt
                    };
                    break;
                }
                else
                {
                    pluginInfoEx = new PluginInfo()
                    {
                        Author = pluginInfo.Author,
                        Error = "",
                        IsEnabled = false,
                        Path = assemblyPath,
                        PluginId = pluginInfo.PluginId,
                        PluginName = pluginInfo.PluginName,
                        Description = pluginInfo.Description,
                        Version = pluginInfo.Version,
                        VersionInt = pluginInfo.VersionInt
                    };
                    break;
                }
            }
        }

        //Console.WriteLine($"Response from the plugin: GetVersion(): {pluginInfoEx.PluginId}");


        // This initiates the unload of the HostAssemblyLoadContext. The actual unloading doesn't happen
        // right away, GC has to kick in later to collect all the stuff.
        alc.Unload();
        return pluginInfoEx;
    }
}