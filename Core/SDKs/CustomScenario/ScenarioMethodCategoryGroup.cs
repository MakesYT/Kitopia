namespace Core.SDKs.CustomScenario;

public class ScenarioMethodCategoryGroup
{
    public static ScenarioMethodCategoryGroup RootScenarioMethodCategoryGroup = new();
    public List<MixinInfo> MixinInfos = new();
    public ScenarioMethodCategoryGroup? Parent { get; set; }
    public string Name { get; set; }

    public string DisplayName { get; set; }

    //
    public Dictionary<string, ScenarioMethodCategoryGroup> Childrens { get; set; } = new();
    public Dictionary<string, ScenarioMethodInfo> Methods { get; set; } = new();
    public ScenarioMethodCategoryGroup? GetParent() => Parent;
    public ScenarioMethodCategoryGroup GetRoot() => Parent?.GetRoot() ?? this;

    public void RemoveMethodsByPluginName(string pluginName)
    {
        Childrens.Remove(pluginName);
        for (var i = 0; i < MixinInfos.Count; i++)
        {
            var target = MixinInfos[i].Target.Split("/");
            var nowScenarioMethodCategoryGroup = RootScenarioMethodCategoryGroup;
            for (var i1 = 0; i1 < target.Length - 1; i1++)
            {
                nowScenarioMethodCategoryGroup = nowScenarioMethodCategoryGroup.Childrens[target[i1]];
            }

            nowScenarioMethodCategoryGroup.Methods.Remove(target.Last());
        }
    }
}

/// 
/// <param name="Target">
/// 其他插件名称/分类1/.../方法绝对名称<see cref="ScenarioMethodInfo.MethodAbsolutelyName"/>
/// </param>
/// 
public struct MixinInfo
{
    public string Source { get; set; }
    public string Target { get; set; }
}