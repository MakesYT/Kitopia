using System.Reflection;

namespace Core.SDKs.Services.Plugin;

public class PluginOverall
{
    public static readonly List<Action<string>> SearchActions = new();

    /// <summary>
    /// 
    /// </summary>
    /// 
    public static readonly Dictionary<string, Dictionary<string, (MethodInfo, object)>> CustomScenarioNodeMethods =
        new();
}