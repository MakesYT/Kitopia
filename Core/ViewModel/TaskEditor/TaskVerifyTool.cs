using CommunityToolkit.Mvvm.Input;
using Core.SDKs.Services.Plugin;

namespace Core.ViewModel.TaskEditor;

public record DoMethod
{
    private int index;
    private string pluginName;
    private string methodName;
    private List<string> inputs;
    private List<string> outputs;
}

public partial class TaskEditorViewModel
{
    Dictionary<string, object> datas = new();
    private List<DoMethod> methods = new();
    List<PointItem> pointItems = new();
    bool finnish = false;
    bool pass = false;

    [RelayCommand]
    private void VerifyNode()
    {
        datas = new();
        pointItems = new();
        finnish = false;
        pass = false;
        pointItems.Add(Nodes[0]);
        var connectionItem = Connections.FirstOrDefault((e) => e.Source == Nodes[0].Output[0]);
        if (connectionItem == null)
        {
            return;
        }

        var firstNodes = connectionItem.Target.Source;
        ParsePointItem(firstNodes);
    }

    public void ParsePointItem(PointItem nowPointItem)
    {
        pointItems.Add(nowPointItem);
        var plugin = PluginManager.EnablePlugin[nowPointItem.Plugin];
        var methodInfos = plugin.GetMethodInfos();
        List<string> input = new();
        List<string> output = new();
        foreach (var connectorItem in nowPointItem.Input)
        {
            if (!connectorItem.IsConnected)
            {
                //当前节点有一个输入参数不存在,验证失败
                break;
            }

            //这是连接当前节点的节点
            var connectionItem = Connections.First((e) => e.Target == connectorItem);
            var sourceSource = connectionItem.Source.Source;
            if (pointItems.Contains(sourceSource))
            {
                //如果包含则证明源节点已被解析
                continue;
            }
        }

        foreach (var connectorItem in nowPointItem.Output)
        {
            if (!connectorItem.IsConnected)
            {
                //当前节点有一个输入参数不存在,则无视
                continue;
            }

            var connectionItem = Connections.First((e) => e.Source == connectorItem);
            var sourceSource = connectionItem.Target.Source;
            if (pointItems.Contains(sourceSource))
            {
                //如果包含则证明源节点已被解析
                continue;
            }

            ParsePointItem(sourceSource);
        }
    }
}