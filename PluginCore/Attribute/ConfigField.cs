using System;

namespace PluginCore.Attribute;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
public class ConfigField : System.Attribute
{
}