#region

using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Core.SDKs.CustomScenario;
using Core.SDKs.Services.Config;
using Core.SDKs.Tools;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PluginCore;
using PluginCore.Attribute;

#endregion

namespace Core.SDKs.Services.Plugin;

public class Plugin
{
    private static readonly ILog Log = LogManager.GetLogger(nameof(Plugin));
    public Assembly? _dll;

    private AssemblyLoadContextH? _plugin;

    public IServiceProvider? ServiceProvider;

    public Plugin(string path)
    {
        _plugin = new AssemblyLoadContextH(path, path.Split("\\").Last() + "_plugin");
        _dll = _plugin.LoadFromAssemblyPath(path);
        var t = _dll.GetExportedTypes();
        Dictionary<string, (MethodInfo, object)> methodInfos = new();
        List<Func<string, SearchViewItem?>> searchViews = new();
        foreach (var type in t)
        {
            if (type.GetInterface("IPlugin") != null)
            {
                PluginInfo = (PluginInfo)type.GetField("PluginInfo").GetValue(null);
                //var instance = Activator.CreateInstance(type);
                ServiceProvider = (IServiceProvider)type.GetMethod("GetServiceProvider").Invoke(null, null);

                ((IPlugin)ServiceProvider.GetService(type)).OnEnabled();
            }

            foreach (var methodInfo in type.GetMethods())
            {
                if (methodInfo.GetCustomAttributes(typeof(PluginMethod)).Any()) //情景的可用节点
                {
                    if (methodInfo.GetParameters()[^1].ParameterType.FullName != "System.Threading.CancellationToken")
                    {
                        continue;
                    }

                    methodInfos.Add(ToMtdString(methodInfo),
                        (methodInfo, GetPointItemByMethodInfo(methodInfo)));
                }

                if (methodInfo.GetCustomAttributes(typeof(SearchMethod)).Any()) //搜索的切入方法
                {
                    searchViews.Add(e =>
                    {
                        var invoke = methodInfo.Invoke(
                            ServiceProvider!.GetService(methodInfo.DeclaringType!),
                            new object?[] { e });
                        if (invoke is null)
                        {
                            return null;
                        }

                        return ((SearchViewItem)invoke);
                    });
                }
            }
        }

        PluginOverall.SearchActions.Add(ToPlgString(), searchViews);
        PluginOverall.CustomScenarioNodeMethods.Add(ToPlgString(), methodInfos);
    }

    public PluginInfo PluginInfo
    {
        set;
        get;
    }

    public string ToPlgString() => $"{PluginInfo.Author}_{PluginInfo.PluginId}";

    public string ToMtdString(MethodInfo methodInfo)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("|");
        foreach (var genericArgument in methodInfo.GetParameters())
        {
            var plugin = PluginManager.EnablePlugin
                .FirstOrDefault((e) => e.Value._dll == genericArgument.ParameterType.Assembly).Value;
            // type.Assembly.
            // var a = PluginManager.GetPlugnNameByTypeName(type.FullName);
            if (plugin is null)
            {
                sb.Append($"System {genericArgument.ParameterType.FullName}");
                sb.Append("|");
                continue;
            }

            sb.Append($"{plugin.ToPlgString()} {genericArgument.ParameterType.FullName}");
            sb.Append("|");
        }

        sb.Remove(sb.Length - 1, 1);
        return $"{PluginInfo.Author}_{PluginInfo.PluginId}/{methodInfo.DeclaringType!.FullName}/{methodInfo.Name}{sb}";
    }

    public bool IsMyType(string typeName)
    {
        foreach (var type in _dll.GetTypes())
        {
            if (type.FullName == typeName)
            {
                return true;
            }
        }

        return false;
        return _dll.GetTypes().Any(t => t.FullName == typeName);
    }

    public Type GetType(string typeName)
    {
        foreach (var type in _dll.GetTypes())
        {
            if (type.FullName == typeName)
            {
                return type;
            }
        }

        return null;
    }

    // private Dictionary<Type, object>? _instance = new();
    public static PluginInfoEx GetPluginInfoEx(string assemblyPath, out WeakReference alcWeakRef)
    {
        var alc = new AssemblyLoadContextH(assemblyPath, "pluginInfo");

        // Create a weak reference to the AssemblyLoadContext that will allow us to detect
        // when the unload completes.
        alcWeakRef = new WeakReference(alc);

        // Load the plugin assembly into the HostAssemblyLoadContext.
        // NOTE: the assemblyPath must be an absolute path.
        var a = alc.LoadFromAssemblyPath(assemblyPath);

        // Get the plugin interface by calling the PluginClass.GetInterface method via reflection.
        var t = a.GetExportedTypes();
        var pluginInfoEx = new PluginInfoEx() { Version = "error" };
        foreach (var type in t)
        {
            if (type.GetInterface("IPlugin") != null)
            {
                var pluginInfo = (PluginInfo)type.GetField("PluginInfo").GetValue(null);

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

        //Console.WriteLine($"Response from the plugin: GetVersion(): {pluginInfoEx.PluginId}");


        // This initiates the unload of the HostAssemblyLoadContext. The actual unloading doesn't happen
        // right away, GC has to kick in later to collect all the stuff.
        alc.Unload();
        return pluginInfoEx;
    }

    private object GetPointItemByMethodInfo(MethodInfo methodInfo)
    {
        var customAttribute = (PluginMethod)methodInfo.GetCustomAttribute(typeof(PluginMethod));
        var pointItem = new PointItem()
        {
            Plugin = this.ToPlgString(),
            MerthodName = ToMtdString(methodInfo),
            Title = $"{PluginInfo.Author}_{PluginInfo.PluginId}_{customAttribute.Name}"
        };
        ObservableCollection<ConnectorItem> inpItems = new();
        inpItems.Add(new ConnectorItem()
        {
            Source = pointItem,
            Type = typeof(NodeConnectorClass),
            Title = "流输入",
            TypeName = "节点"
        });
        int autoUnboxIndex = 0;
        for (var index = 0; index < methodInfo.GetParameters().Length; index++)
        {
            var parameterInfo = methodInfo.GetParameters()[index];
            if (parameterInfo.ParameterType.FullName == "System.Threading.CancellationToken")
            {
                continue;
            }

            if (parameterInfo.ParameterType.GetCustomAttribute(typeof(AutoUnbox)) is not null)
            {
                autoUnboxIndex++;
                var type = parameterInfo.ParameterType;
                foreach (var memberInfo in type.GetProperties())
                {
                    List<string>? interfaces = null;
                    if (!memberInfo.PropertyType.FullName.StartsWith("System."))
                    {
                        interfaces = new();
                        foreach (var @interface in memberInfo.PropertyType.GetInterfaces())
                        {
                            interfaces.Add(@interface.FullName);
                        }
                    }


                    inpItems.Add(new ConnectorItem()
                    {
                        Source = pointItem,
                        Type = memberInfo.PropertyType,
                        AutoUnboxIndex = autoUnboxIndex,
                        Interfaces = interfaces,
                        Title = customAttribute.GetParameterName(memberInfo.Name),
                        TypeName = BaseNodeMethodsGen.GetI18N(memberInfo.PropertyType.FullName),
                    });
                }
            }
            else
            {
                inpItems.Add(new ConnectorItem()
                {
                    Source = pointItem,
                    Type = parameterInfo.ParameterType,
                    Title = customAttribute.GetParameterName(parameterInfo.Name),
                    TypeName = BaseNodeMethodsGen.GetI18N(parameterInfo.ParameterType.FullName)
                });
            }

            //Log.Debug($"参数{index}:类型为{parameterInfo.ParameterType}");
        }

        if (methodInfo.ReturnParameter.ParameterType != typeof(void))
        {
            ObservableCollection<ConnectorItem> outItems = new();
            if (methodInfo.ReturnParameter.ParameterType.GetCustomAttribute(typeof(AutoUnbox)) is not null)
            {
                autoUnboxIndex++;
                var type = methodInfo.ReturnParameter.ParameterType;
                foreach (var memberInfo in type.GetProperties())
                {
                    List<string>? interfaces = null;
                    if (!memberInfo.PropertyType.FullName.StartsWith("System."))
                    {
                        interfaces = new();
                        foreach (var @interface in memberInfo.PropertyType.GetInterfaces())
                        {
                            interfaces.Add(@interface.FullName);
                        }
                    }

                    outItems.Add(new ConnectorItem()
                    {
                        Source = pointItem,
                        Type = memberInfo.PropertyType,
                        AutoUnboxIndex = autoUnboxIndex,
                        Interfaces = interfaces,
                        Title = customAttribute.GetParameterName(memberInfo.Name),
                        TypeName = BaseNodeMethodsGen.GetI18N(memberInfo.PropertyType.FullName),
                        IsOut = true
                    });
                }
            }
            else
            {
                List<string> interfaces = new();
                foreach (var @interface in methodInfo.ReturnParameter.ParameterType.GetInterfaces())
                {
                    interfaces.Add(@interface.FullName);
                }


                outItems.Add(new ConnectorItem()
                {
                    Source = pointItem,
                    Type = methodInfo.ReturnParameter.ParameterType,
                    Title = customAttribute.GetParameterName("return"),
                    Interfaces = interfaces,
                    TypeName =
                        BaseNodeMethodsGen.GetI18N(methodInfo.ReturnParameter.ParameterType.FullName),
                    IsOut = true
                });
            }


            pointItem.Output = outItems;
        }


        pointItem.Input = inpItems;

        return pointItem;
    }

    public List<FieldInfo> GetFieldInfos()
    {
        var _fieldInfos = new List<FieldInfo>();
        var t = _dll.GetExportedTypes();
        foreach (var type in t)
        {
            foreach (var fieldInfo in type.GetFields())
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
    public static void LoadBypath(string name, string path) =>
        PluginManager.EnablePlugin.Add(name,
            new Plugin(path));

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

        PluginOverall.SearchActions.Remove($"{PluginInfo.Author}_{PluginInfo.PluginId}");
        PluginOverall.CustomScenarioNodeMethods.Remove($"{PluginInfo.Author}_{PluginInfo.PluginId}");
        PluginInfo = new PluginInfo();
        ServiceProvider = null;

        _plugin.Unload();
        weakReference = new WeakReference(_plugin);
    }
}