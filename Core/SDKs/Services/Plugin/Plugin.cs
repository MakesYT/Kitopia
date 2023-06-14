using System.Reflection;
using Newtonsoft.Json.Linq;
using PluginCore;
using PluginCore.Attribute;

namespace Core.SDKs.Services.Plugin;

public class Plugin
{
    public PluginInfo _pluginInfo;
    private Assembly _plugin;
    private IPlugin _main;
    private List<MethodInfo> _methodInfos = new();
    private List<FieldInfo> _fieldInfos = new();

    public Plugin(string path)
    {
        _plugin = Assembly.LoadFrom(path);
        Type[] t = _plugin.GetExportedTypes();
        foreach (Type type in t)
        {
            if (type.GetInterface("IPlugin") != null)
            {
                IPlugin show = (IPlugin)Activator.CreateInstance(type);
                _main = show;
                _pluginInfo = show.PluginInfo();
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