using System;

namespace PluginCore.Attribute;

[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = true)]
public class SelfInput : System.Attribute
{
}