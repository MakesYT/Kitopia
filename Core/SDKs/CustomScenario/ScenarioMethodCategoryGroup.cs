using PluginCore.Attribute.Scenario;

namespace Core.SDKs.CustomScenario;

public class ScenarioMethodCategoryGroup
{
    public static ScenarioMethodCategoryGroup RootScenarioMethodCategoryGroup = new();

    public static ScenarioMethodCategoryGroup GetScenarioMethodCategoryGroupByAttribute(
        ScenarioMethodCategoryAttribute attribute, ScenarioMethodCategoryGroup? scenarioMethodCategoryGroup = null)
    {
        var strings = attribute.Name.Split("/");
        var nowScenarioMethodCategoryGroup = attribute.IsMixinOrTopCategory
            ? RootScenarioMethodCategoryGroup
            : scenarioMethodCategoryGroup ?? RootScenarioMethodCategoryGroup;
        for (var index = 0; index < strings.Length; index++)
        {
            var se = strings[index];
            if (nowScenarioMethodCategoryGroup.Childrens.ContainsKey(se))
            {
                nowScenarioMethodCategoryGroup = nowScenarioMethodCategoryGroup.Childrens[se];
            }
            else
            {
                var newScenarioMethodCategoryGroup = new ScenarioMethodCategoryGroup()
                {
                    Name = se,
                    Parent = nowScenarioMethodCategoryGroup
                };
                nowScenarioMethodCategoryGroup = newScenarioMethodCategoryGroup;
                if (index == strings.Length - 1)
                {
                    nowScenarioMethodCategoryGroup.DisplayName = attribute.Name;
                }

                nowScenarioMethodCategoryGroup.Childrens.Add(se, nowScenarioMethodCategoryGroup);
            }
        }

        return nowScenarioMethodCategoryGroup;
    }

    public List<MixinInfo> MixinInfos = new();
    public ScenarioMethodCategoryGroup? Parent { get; set; }
    public string Name { get; set; }

    public string DisplayName { get; set; }

    //
    public Dictionary<string, ScenarioMethodCategoryGroup> Childrens { get; set; } = new();
    public Dictionary<string, ScenarioMethodNode> Methods { get; set; } = new();
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
                if (nowScenarioMethodCategoryGroup.Childrens.ContainsKey(target[i1]))
                {
                    nowScenarioMethodCategoryGroup = nowScenarioMethodCategoryGroup.Childrens[target[i1]];
                }
                else throw new ScenarioException("路径不存在");
            }

            nowScenarioMethodCategoryGroup.Methods.Remove(target.Last());
        }
    }
}

/// 
/// <param name="Target">
/// 其他插件名称/分类1/.../方法绝对名称<see cref="ScenarioMethod.MethodAbsolutelyName"/>
/// </param>
/// 
public struct MixinInfo
{
    public string Source { get; set; }
    public string Target { get; set; }
}