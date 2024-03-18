#region

using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Core.SDKs.CustomScenario;
using Core.SDKs.HotKey;
using Core.SDKs.Services.Config;
using Core.SDKs.Tools;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PluginCore;
using PluginCore.Attribute;
using PluginCore.Config;

#endregion

namespace Core.SDKs.Services.Plugin;

public class Plugin
{
    private static readonly ILog Log = LogManager.GetLogger(nameof(Plugin));

    private AssemblyLoadContextH? _plugin;

    public IServiceProvider? ServiceProvider;
    public  void AddConfig(string key,ConfigBase configBase)
    {
        var configF =
            new FileInfo($"{AppDomain.CurrentDomain.BaseDirectory}configs{Path.DirectorySeparatorChar}{key}.json");
        if (!configF.Exists)
        {
            var j = JsonConvert.SerializeObject(configBase, Formatting.Indented);
            File.WriteAllText(configF.FullName, j);
        }

        var json = File.ReadAllText(configF.FullName);
        try
        {
            var deserializeObject =  _plugin.Assemblies.FirstOrDefault(x => x.GetName().Name == "Newtonsoft.Json")?.GetType("Newtonsoft.Json.JsonConvert").GetMethod("DeserializeObject")?.Invoke(null, new object[] { json, configBase.GetType() })! as ConfigBase ?? configBase;
            
            if (!ConfigManger.Configs.TryAdd(key,deserializeObject))
            {
                ConfigManger.Configs[key] = deserializeObject;
            }

            var type = deserializeObject.GetType().BaseType;
            var fieldInfo = type.GetField("Instance");
            fieldInfo.SetValue(deserializeObject,deserializeObject);
            deserializeObject.AfterLoad();
        }
        catch (Exception e)
        {
            Log.Error(e);
            Log.Error("配置文件加载失败");
        }
        configBase.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public).ToList().ForEach(x =>
        {
            if (x.GetCustomAttribute<ConfigField>() is { } configField)
            {
                if (configField.FieldType==ConfigFieldType.快捷键)
                {
                    HotKeyManager.HotKeys.Add(x.GetValue(configBase) as HotKeyModel);
                }
            }
        });
    }
    public Plugin(string path)
    {
        _plugin = new AssemblyLoadContextH(path, path.Split(Path.DirectorySeparatorChar).Last() + "_plugin");
        Log.Debug($"加载插件:{path}");
        var t = _dll.GetExportedTypes();
        Dictionary<string, (MethodInfo, object)> methodInfos = new();
        List<Func<string, SearchViewItem?>> searchViews = new();
        foreach (var type in t)
        {
            if (type.GetInterface("IPlugin") != null)
            {
                var PluginInfo = (PluginInfo)type.GetField("PluginInfo").GetValue(null);
                PluginInfo.Path = path;
                this.PluginInfo = PluginInfo;
                Log.Debug($"加载插件:{PluginInfo.ToPlgString()}");
                //var instance = Activator.CreateInstance(type);
                ServiceProvider = (IServiceProvider)type.GetMethod("GetServiceProvider").Invoke(null, null);

                ((IPlugin)ServiceProvider.GetService(type)).OnEnabled(ServiceProvider);
                break;
            }

           
            
        }

        foreach (var type in t)
        {
            if (type.BaseType==typeof(ConfigBase))
            {
                var instance =(ConfigBase) Activator.CreateInstance(type);
                instance.Name = $"{PluginInfo.ToPlgString()}#{type.FullName}";
                AddConfig($"{PluginInfo.ToPlgString()}#{type.FullName}", instance);
            }
            if (typeof(CustomScenarioTrigger).IsAssignableFrom(type))
            {
                var fieldInfo = type.GetField("Info");
                var customScenarioTriggerInfo = (CustomScenarioTriggerInfo)(fieldInfo is null
                    ? new CustomScenarioTriggerInfo { Name = $"{PluginInfo.ToPlgString()}_{type.Name}" }
                    : fieldInfo.GetValue(null)!);
                customScenarioTriggerInfo.PluginInfo = PluginInfo.ToPlgString();
                CustomScenarioGloble.Triggers.Add($"{PluginInfo.ToPlgString()}_{type.Name}",
                    customScenarioTriggerInfo);
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

    public Assembly? _dll => _plugin.Assembly;

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

            bool IsSelf = parameterInfo.GetCustomAttributes(typeof(SelfInput)).Any();

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
                        IsSelf = IsSelf,
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
                    IsSelf = IsSelf,
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

    public JObject GetConfigJObject()
    {
        var jObject = new JObject();


        return jObject;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void UnloadByPluginInfo(string pluginInfoEx, out WeakReference weakReference)
    {
        if (PluginManager.EnablePlugin.ContainsKey(pluginInfoEx))
        {
            {
                PluginManager.EnablePlugin[pluginInfoEx].Unload(out weakReference);

                return;
            }
        }

        weakReference = new WeakReference(null);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void Load(PluginInfo pluginInfoEx)
    {
        PluginManager.EnablePlugin.Add(pluginInfoEx.ToPlgString(),
            new Plugin(pluginInfoEx.Path));
    }

    public void Unload(out WeakReference weakReference)
    {
        Log.Debug($"卸载插件:{PluginInfo.ToPlgString()}");
        ConfigManger.RemoveConfig($"{PluginInfo.ToPlgString()}");

        PluginOverall.SearchActions.Remove($"{PluginInfo.ToPlgString()}");
        PluginOverall.CustomScenarioNodeMethods.Remove($"{PluginInfo.ToPlgString()}");
        var keyValuePairs = CustomScenarioGloble.Triggers.Where(e => e.Value.PluginInfo == PluginInfo.ToPlgString());
        foreach (var keyValuePair in keyValuePairs)
        {
            CustomScenarioGloble.Triggers.Remove(keyValuePair.Key);
        }

        keyValuePairs = null;


        CustomScenarioManger.UnloadByPlugStr(PluginInfo.ToPlgString());

        PluginInfo = new PluginInfo();
        ServiceProvider = null;

        _plugin.Unload();
        weakReference = new WeakReference(_plugin);
    }
}