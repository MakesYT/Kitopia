﻿using Core.SDKs.CustomType;
using PluginCore;

namespace Core.SDKs.CustomScenario;

public class CustomScenarioGloble
{
    public static Dictionary<string, string> _i18n = new()
    {
        { "System.String", "字符串" },
        { "System.Boolean", "布尔" },
        { "System.Int32", "整数" },
        { "System.Double", "浮点" },
        { "System.Object", "任意" },
        { "PluginCore.NodeConnectorClass", "节点" },
    };

    public static ObservableDictionary<string, CustomScenarioTriggerInfo> Triggers = new()
    {
        { "Kitopia_SoftwareStarted", new CustomScenarioTriggerInfo() { Name = "Kitopia程序启动时" } },
        {
            "Kitopia_SoftwareShutdown",
            new CustomScenarioTriggerInfo() { Name = "Kitopia程序关闭时", Description = "注意该触发器不会进入Tick" }
        },
    };
}