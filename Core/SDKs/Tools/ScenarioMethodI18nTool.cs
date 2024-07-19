using Core.SDKs.CustomScenario;

namespace Core.SDKs.Tools;

public class ScenarioMethodI18nTool
{
    private static readonly Dictionary<string, Type> _baseType = new()
    {
        { "字符串", typeof(string) },
        { "布尔", typeof(bool) },
        { "整型", typeof(int) },
        { "双精度浮点数", typeof(double) },
    };

    public static string GetI18N(string key)
    {
        if (CustomScenarioGloble._i18n.TryGetValue(key, out var n))
        {
            return n;
        }

        return key;
    }
}