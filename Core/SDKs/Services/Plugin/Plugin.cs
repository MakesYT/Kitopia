using System.Reflection;
using Core.SDKs.Services.Config;
using Newtonsoft.Json;
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

    private AssemblyLoadContextH _plugin;

    private List<MethodInfo>? _methodInfos = new();
    private List<FieldInfo>? _fieldInfos = new();

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

        // Create a weak reference to the AssemblyLoadContext that will allow us to detect
        // when the unload completes.

        // Load the plugin assembly into the HostAssemblyLoadContext.
        // NOTE: the assemblyPath must be an absolute path.
        Assembly a = _plugin.LoadFromAssemblyPath(path);

        // Get the plugin interface by calling the PluginClass.GetInterface method via reflection.
        var t = a.GetExportedTypes();
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
                if (methodInfo.GetCustomAttributes(typeof(PluginMethod)).Any())
                {
                    _methodInfos.Add(methodInfo);
                }
            }

            foreach (FieldInfo fieldInfo in type.GetFields())
            {
                if (fieldInfo.GetCustomAttributes(typeof(ConfigField)).Any())
                {
                    var customAttribute = (ConfigField)fieldInfo.GetCustomAttribute(typeof(ConfigField))!;
                    // fieldInfo.SetValue(null,default());
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
            jObject.Add(fieldInfo.Name,
                new JValue(fieldInfo.GetValue(ServiceProvider.GetService(fieldInfo.DeclaringType))));
        }

        return jObject;
    }

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
        _methodInfos = null;
        _fieldInfos = null;

        ServiceProvider = null;

        _plugin.Unload();
        weakReference = new WeakReference(_plugin);
    }

    public List<PluginSettingItem> GetConfigSettingItems()
    {
        List<PluginSettingItem> settingItems = new();
        foreach (var fieldInfo in _fieldInfos)
        {
            var pluginSettingItem = new PluginSettingItem()
            {
            };
            settingItems.Add(pluginSettingItem);
        }

        return settingItems;
    }
}