using System;

namespace PluginCore.Attribute;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class PluginMethod : System.Attribute
{
}