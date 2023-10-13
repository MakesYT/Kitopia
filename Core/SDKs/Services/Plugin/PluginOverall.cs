using System.Reflection;

namespace Core.SDKs.Services.Plugin;

public class PluginOverall
{
    public static readonly Dictionary<string, List<Func<string, SearchViewItem?>>> SearchActions = new();

    /// <summary>
    /// 第一个字典的key格式为$"{PluginInfo.Author}_{PluginInfo.PluginId}"
    /// 第二个字典的key格式为$"{type.FullName}_{methodInfo.Name}"
    ///           value第二个为PointItem
    /// </summary>
    /// 
    public static readonly Dictionary<string, Dictionary<string, (MethodInfo, object)>> CustomScenarioNodeMethods =
        new();
}