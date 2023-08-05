using System;

namespace PluginCore.Attribute;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class PluginMethod : System.Attribute
{
    public string Name
    {
        get;
        set;
    }

    public PluginMethod(string name)
    {
        Name = name;
    }
}