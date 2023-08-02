using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using Core.SDKs.Services.Config;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PluginCore;
using PluginCore.Attribute;

namespace Core.SDKs.Services.Plugin;

public class Plugin
{
    private static readonly ILog Log = LogManager.GetLogger(nameof(Plugin));

    public PluginInfo PluginInfo
    {
        set;
        get;
    }

    private AssemblyLoadContextH? _plugin;
    private Assembly? _dll;

    private IServiceProvider? ServiceProvider;

    // private Dictionary<Type, object>? _instance = new();
    public static PluginInfoEx GetPluginInfoEx(string assemblyPath, out WeakReference alcWeakRef)
    {
        var alc = new AssemblyLoadContextH(assemblyPath, "pluginInfo");

        // Create a weak reference to the AssemblyLoadContext that will allow us to detect
        // when the unload completes.
        alcWeakRef = new WeakReference(alc);

        // Load the plugin assembly into the HostAssemblyLoadContext.
        // NOTE: the assemblyPath must be an absolute path.
        Assembly a = alc.LoadFromAssemblyPath(assemblyPath);

        // Get the plugin interface by calling the PluginClass.GetInterface method via reflection.
        var t = a.GetExportedTypes();
        PluginInfoEx pluginInfoEx = new PluginInfoEx() { Version = "error" };
        foreach (Type type in t)
        {
            if (type.GetInterface("IPlugin") != null)
            {
                var pluginInfo = (PluginInfo)(type.GetField("PluginInfo").GetValue(null));

                if (ConfigManger.Config.EnabledPluginInfos.Contains(pluginInfo))
                {
                    pluginInfoEx = new PluginInfoEx()
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
                    pluginInfoEx = new PluginInfoEx()
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
                    pluginInfoEx = new PluginInfoEx()
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

        Console.WriteLine($"Response from the plugin: GetVersion(): {pluginInfoEx.PluginId}");


        // This initiates the unload of the HostAssemblyLoadContext. The actual unloading doesn't happen
        // right away, GC has to kick in later to collect all the stuff.
        alc.Unload();
        return pluginInfoEx;
    }

    public Plugin(string path)
    {
        _plugin = new AssemblyLoadContextH(path, path.Split("\\").Last() + "_plugin");
        _dll = _plugin.LoadFromAssemblyPath(path);
    }

    public List<MethodInfo> GetMethodInfos()
    {
        var methodInfos = new List<MethodInfo>();
        var t = _dll.GetExportedTypes();
        foreach (Type type in t)
        {
            if (type.GetInterface("IPlugin") != null)
            {
                PluginInfo = (PluginInfo)type.GetField("PluginInfo").GetValue(null);
                //var instance = Activator.CreateInstance(type);
                ServiceProvider = (IServiceProvider)type.GetMethod("GetServiceProvider").Invoke(null, null);

                ((PluginCore.IPlugin)ServiceProvider.GetService(type)).OnEnabled();
            }

            foreach (MethodInfo methodInfo in type.GetMethods())
            {
                if (!methodInfo.GetCustomAttributes(typeof(PluginMethod)).Any())
                {
                    continue;
                }

                Log.Debug($"找到方法{methodInfo.Name}");
                for (var index = 0; index < methodInfo.GetParameters().Length; index++)
                {
                    var parameterInfo = methodInfo.GetParameters()[index];
                    Log.Debug($"参数{index}:类型为{parameterInfo.ParameterType}");
                }

                Log.Debug($"输出:类型为{methodInfo.ReturnParameter.ParameterType}");


                methodInfos.Add(methodInfo);
            }
        }

        return methodInfos;
    }

    public List<FieldInfo> GetFieldInfos()
    {
        var _fieldInfos = new List<FieldInfo>();
        var t = _dll.GetExportedTypes();
        foreach (Type type in t)
        {
            if (type.GetInterface("IPlugin") != null)
            {
                PluginInfo = (PluginInfo)type.GetField("PluginInfo").GetValue(null);
                //var instance = Activator.CreateInstance(type);
                ServiceProvider = (IServiceProvider)type.GetMethod("GetServiceProvider").Invoke(null, null);

                ((PluginCore.IPlugin)ServiceProvider.GetService(type)).OnEnabled();
            }

            foreach (FieldInfo fieldInfo in type.GetFields())
            {
                if (!fieldInfo.GetCustomAttributes(typeof(ConfigField)).Any())
                {
                    continue;
                }

                Log.Debug($"找到属性{fieldInfo.Name}");
                _fieldInfos.Add(fieldInfo);
            }
        }

        return _fieldInfos;
    }

    public JObject GetConfigJObject()
    {
        var jObject = new JObject();


        return jObject;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void LoadBypath(string name, string path)
    {
        PluginManager.EnablePlugin.Add(name,
            new Plugin(path));
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void UnloadByPluginInfo(PluginInfoEx pluginInfoEx, out WeakReference weakReference)
    {
        if (PluginManager.EnablePlugin.TryGetValue($"{pluginInfoEx.Author}_{pluginInfoEx.PluginId}",
                out var plugin))
        {
            pluginInfoEx.IsEnabled = false;

            {
                PluginManager.EnablePlugin.Remove($"{pluginInfoEx.Author}_{pluginInfoEx.PluginId}");
                plugin.Unload(out weakReference);
                return;
            }
        }

        weakReference = new WeakReference(null);
    }

    public void Unload(out WeakReference weakReference)
    {
        var config1 = new FileInfo(AppDomain.CurrentDomain.BaseDirectory +
                                   $"configs\\{PluginInfo.Author}_{PluginInfo.PluginId}.json");
        File.WriteAllText(config1.FullName,
            JsonConvert.SerializeObject(GetConfigJObject(), Formatting.Indented));
        _dll = null;

        ServiceProvider = null;

        _plugin.Unload();
        _plugin = null;
        weakReference = new WeakReference(_plugin);
    }
}