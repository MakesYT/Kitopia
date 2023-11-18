﻿using PluginCore;

namespace KitopiaEx.CustomScenarioTriggers;

public class Trigger1 : CustomScenarioTrigger
{
    //触发器信息
    public static CustomScenarioTriggerInfo Info = new CustomScenarioTriggerInfo
    {
        Name = "Trigger1",
        Description = "触发器1"
    };

    public static void Excite1()
    {
        Excite(KitopiaEx.PluginInfo, nameof(Trigger1));
    }
}