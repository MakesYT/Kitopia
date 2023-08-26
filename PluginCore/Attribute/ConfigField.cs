#region

using System;

#endregion

namespace PluginCore.Attribute;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
public class ConfigField : System.Attribute
{
    public string Tittle
    {
        get;
        set;
    }

    public string Description
    {
        get;
        set;
    }

    public object Setting
    {
        get;
        set;
    }

    public int Symbol
    {
        get;
        set;
    }

    public ConfigField(string title, string description, int symbol = 0)
    {
        Tittle = title;
        Description = description;
        Symbol = symbol;
    }
}