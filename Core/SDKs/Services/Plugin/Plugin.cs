#region

using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Core.SDKs.CustomScenario;
using Core.SDKs.HotKey;
using Core.SDKs.Services.Config;
using log4net;
using PluginCore;
using PluginCore.Attribute;
using PluginCore.Attribute.Scenario;
using PluginCore.Config;

#endregion

namespace Core.SDKs.Services.Plugin;

public class Plugin
{
    private static readonly ILog Log = LogManager.GetLogger(nameof(Plugin));

    private AssemblyLoadContextH? _plugin;

    public IServiceProvider? ServiceProvider;

    public void AddConfig(string key, ConfigBase configBase)
    {
        var configF =
            new FileInfo($"{AppDomain.CurrentDomain.BaseDirectory}configs{Path.DirectorySeparatorChar}{key}.json");
        if (!configF.Exists)
        {
            var j = JsonSerializer.Serialize(configBase, configBase.GetType(), ConfigManger.DefaultOptions);
            File.WriteAllText(configF.FullName, j);
        }

        var json = File.ReadAllText(configF.FullName);
        try
        {
            var deserializeObject =
                JsonSerializer.Deserialize(json, configBase.GetType(), ConfigManger.DefaultOptions)! as ConfigBase ??
                configBase;
            if (!ConfigManger.Configs.TryAdd(key, deserializeObject))
            {
                ConfigManger.Configs[key] = deserializeObject;
            }

            deserializeObject.GetType()
                .BaseType.GetField("Instance")
                .SetValue(deserializeObject, deserializeObject);
            deserializeObject.AfterLoad();
        }
        catch (Exception e)
        {
            Log.Error(e);
            Log.Error("配置文件加载失败");
        }

        configBase.GetType()
            .GetFields(BindingFlags.Instance | BindingFlags.Public)
            .ToList()
            .ForEach(x =>
            {
                if (x.GetCustomAttribute<ConfigField>() is { } configField)
                {
                    if (configField.FieldType == ConfigFieldType.快捷键)
                    {
                        HotKeyManager.HotKeys.Add(x.GetValue(configBase) as HotKeyModel);
                    }
                }
            });
    }

    public Plugin(string path)
    {
        _plugin = new AssemblyLoadContextH(path, path.Split(Path.DirectorySeparatorChar)
            .Last() + "_plugin");
        Log.Debug($"加载插件:{path}");
        var t = _dll.GetExportedTypes();
        //Dictionary<string, (MethodInfo, object)> methodInfos = new();
        ScenarioMethodCategoryGroup pluginMainScenarioMethodCategoryGroup = new();

        List<Func<string, SearchViewItem?>> searchViews = new();
        foreach (var type in t)
        {
            if (type.GetInterface("IPlugin") != null)
            {
                var PluginInfo = (PluginInfo)type.GetField("PluginInfo")
                    .GetValue(null);
                PluginInfo.Path = path;
                this.PluginInfo = PluginInfo;
                Log.Debug($"加载插件:{PluginInfo.ToPlgString()}");
                //var instance = Activator.CreateInstance(type);
                ServiceProvider = (IServiceProvider)type.GetMethod("GetServiceProvider")
                    .Invoke(null, null);

                ((IPlugin)ServiceProvider.GetService(type)).OnEnabled(ServiceProvider);
                break;
            }
        }

        if (PluginInfo is null)
        {
            Log.Error("未找到插件信息");
            return;
        }

        pluginMainScenarioMethodCategoryGroup.Name = PluginInfo.PluginName;

        foreach (var type in t)
        {
            if (type.BaseType == typeof(ConfigBase))
            {
                var instance = (ConfigBase)Activator.CreateInstance(type);
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
                ScenarioMethodCategoryGroup scenarioMethodCategoryGroup = pluginMainScenarioMethodCategoryGroup;
                if (type.GetCustomAttribute<ScenarioMethodCategoryAttribute>() is { } scenarioMethodCategoryAttribute)
                {
                    scenarioMethodCategoryGroup =
                        ScenarioMethodCategoryGroup.GetScenarioMethodCategoryGroupByAttribute(
                            scenarioMethodCategoryAttribute, pluginMainScenarioMethodCategoryGroup);
                }

                if (methodInfo.GetCustomAttribute<ScenarioMethodAttribute>() is { } scenarioMethodAttribute) //情景的可用节点
                {
                    if (methodInfo.GetParameters()[^1].ParameterType.FullName != "System.Threading.CancellationToken")
                    {
                        continue;
                    }

                    var scenarioMethodInfo = new ScenarioMethod(methodInfo, PluginInfo, scenarioMethodAttribute,
                        ScenarioMethodType.插件方法);
                    scenarioMethodCategoryGroup.Methods.Add(scenarioMethodInfo.MethodAbsolutelyName,
                        scenarioMethodInfo);
                }

                if (methodInfo.GetCustomAttributes(typeof(SearchMethod))
                    .Any()) //搜索的切入方法
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

        PluginOverall.SearchActions.Add(PluginInfo.ToPlgString(), searchViews);
    }

    public Assembly? _dll => _plugin.Assembly;

    public PluginInfo PluginInfo { set; get; }


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


    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void UnloadByPluginInfo(string pluginInfoEx, out WeakReference weakReference)
    {
        if (PluginManager.EnablePlugin.ContainsKey(pluginInfoEx))
        {
            {
                PluginManager.EnablePlugin[pluginInfoEx]
                    .Unload(out weakReference);

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
        ScenarioMethodCategoryGroup.RootScenarioMethodCategoryGroup.RemoveMethodsByPluginName(PluginInfo.ToPlgString());
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