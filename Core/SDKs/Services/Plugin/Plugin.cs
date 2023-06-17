using System.Reflection;
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

    public static PluginInfo GetPluginInfo(string path)
    {
        var _plugin = Assembly.LoadFrom(path);
        Type[] t = _plugin.GetExportedTypes();
        foreach (Type type in t)
        {
            if (type.GetInterface("IPlugin") != null)
            {
                return (PluginInfo)type.GetMethod("PluginInfo").Invoke(null, null);
            }
        }

        return new PluginInfo();
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