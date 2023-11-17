using PluginCore;

namespace KitopiaEx.CustomScenarioTriggers;

public class Trigger1 : CustomScenarioTrigger
{
    public static string Name = "KitopiaEx自定义触发器";

    public static void Excite1()
    {
        Excite(KitopiaEx.PluginInfo, nameof(Trigger1));
    }
}