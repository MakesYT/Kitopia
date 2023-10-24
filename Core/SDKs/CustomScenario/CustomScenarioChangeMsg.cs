using Core.ViewModel.TaskEditor;

namespace Core.SDKs.CustomScenario;

public class CustomScenarioChangeMsg
{
    public ConnectorItem? ConnectorItem;
    public CustomScenario CustomScenario;

    public string Name;
    public PointItem PointItem;

    /// <summary>
    /// 0来自连接器的修改
    /// 1来自情景属性的修改
    /// </summary>
    public int Type = 0;
}