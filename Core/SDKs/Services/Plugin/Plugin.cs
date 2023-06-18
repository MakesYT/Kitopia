using System.Reflection;
using Core.SDKs.Services.Config;
using Newtonsoft.Json.Linq;
using PluginCore;
using PluginCore.Attribute;

namespace Core.SDKs.Services.Plugin;

public class Plugin
{
    public PluginInfo PluginInfo
    {
        set;
        get;
    }

    private readonly Assembly _plugin;
    private IPlugin _main;
    public readonly List<MethodInfo> MethodInfos = new();
    private readonly List<FieldInfo> _fieldInfos = new();

    public static PluginInfoEx GetPluginInfoEx(string assemblyPath, out WeakReference alcWeakRef)
    {
        var alc = new AssemblyLoadContextH(assemblyPath);

        // Create a weak reference to the AssemblyLoadContext that will allow us to detect
        // when the unload completes.
        alcWeakRef = new WeakReference(alc);

        // Load the plugin assembly into the HostAssemblyLoadContext.
        // NOTE: the assemblyPath must be an absolute path.
        Assembly a = alc.LoadFromAssemblyPath(assemblyPath);

        // Get the plugin interface by calling the PluginClass.GetInterface method via reflection.
        var t = a.GetExportedTypes();
        PluginInfoEx pluginInfoEx = new PluginInfoEx() { Author = "error" };
        foreach (Type type in t)
        {
            if (type.GetInterface("IPlugin") != null)
            {
                var pluginInfo = (PluginInfo)(type.GetMethod("PluginInfo").Invoke(null, null));
                if (ConfigManger.Config.EnabledPluginInfos.Contains(pluginInfo))
                {
                    pluginInfoEx = new PluginInfoEx()
                    {
                        Author = pluginInfo.Author,
                        Error = "",
                        IsEnabled = true,
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
                    pluginInfoEx = new PluginInfoEx()
                    {
                        Author = pluginInfo.Author,
                        Error = "插件版本不一致",
                        IsEnabled = false,
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
                    pluginInfoEx = new PluginInfoEx()
                    {
                        Author = pluginInfo.Author,
                        Error = "",
                        IsEnabled = false,
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

        Console.WriteLine($"Response from the plugin: GetVersion(): {pluginInfoEx.PluginId}");


        // This initiates the unload of the HostAssemblyLoadContext. The actual unloading doesn't happen
        // right away, GC has to kick in later to collect all the stuff.
        alc.Unload();
        return pluginInfoEx;
    }

    public Plugin(string path)
    {
        _plugin = Assembly.LoadFrom(path);
        Type[] t = _plugin.GetExportedTypes();
        foreach (Type type in t)
        {
            if (type.GetInterface("IPlugin") != null)
            {
                IPlugin show = (IPlugin)(type);
                _main = show;
                PluginInfo = (PluginInfo)type.GetMethod("PluginInfo").Invoke(null, null);
            }

            foreach (MethodInfo methodInfo in type.GetMethods())
            {
                if (methodInfo.GetCustomAttributes(typeof(PluginMethod)).Any())
                {
                    MethodInfos.Add(methodInfo);
                }
            }

            foreach (FieldInfo fieldInfo in type.GetFields())
            {
                if (fieldInfo.GetCustomAttributes(typeof(ConfigField)).Any())
                {
                    _fieldInfos.Add(fieldInfo);
                }
            }
        }
    }

    public JObject GetConfigJObject()
    {
        var jObject = new JObject();
        foreach (var fieldInfo in _fieldInfos)
        {
            jObject.Add(fieldInfo.Name, new JValue(fieldInfo.GetValue(_plugin)));
        }

        return jObject;
    }
}