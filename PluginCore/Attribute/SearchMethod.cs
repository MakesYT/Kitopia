#region

using System;

#endregion

namespace PluginCore.Attribute;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class SearchMethod : System.Attribute
{
}