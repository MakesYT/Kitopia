﻿using PluginCore;

namespace Core.SDKs.CustomScenario;

public enum CustomScenarioLoadFromJsonFailedType
{
    插件未找到,
    插件未启用,
    方法未找到,
    类未找到
}
public class CustomScenarioLoadFromJsonException : Exception
{
    private string? _pluginName;
    public CustomScenarioLoadFromJsonFailedType FailedType { get; set; }

    public string PluginName
    {
        get;
        set;
    }

   
    public string? MethodName
    {
        get;
        set;
    }

    public CustomScenarioLoadFromJsonException(CustomScenarioLoadFromJsonFailedType customScenarioLoadFromJsonFailedType,string pluginName, string? methodName)
    {
        this.PluginName = pluginName;
        this.MethodName = methodName;
        this.FailedType = customScenarioLoadFromJsonFailedType;
    }
}