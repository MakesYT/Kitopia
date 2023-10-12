#region

using System;
using System.Collections.Generic;
using System.Linq;

#endregion

namespace PluginCore.Attribute;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class PluginMethod : System.Attribute
{
    public string Name
    {
        get;
        set;
    }

    public Dictionary<string, string> ParameterName
    {
        get;
        set;
    }

    public PluginMethod(string name, params string[] parameterName)
    {
        Name = name;
        ParameterName = parameterName
            .Select(s => s.Split('='))
            .ToDictionary(s => s[0], s => s[1]);
    }

    public string GetParameterName(string key)
    {
        if (ParameterName.TryGetValue(key, out var name))
        {
            return name;
        }

        return key;
    }
}