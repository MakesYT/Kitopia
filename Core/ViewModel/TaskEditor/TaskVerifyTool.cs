#region

using System.Reflection;
using CommunityToolkit.Mvvm.Input;
using Core.SDKs.Services.Plugin;
using PluginCore.Attribute;
using Vanara.Extensions.Reflection;

#endregion

namespace Core.ViewModel.TaskEditor;

public partial class TaskEditorViewModel
{
    private List<PointItem> _firstVerifyPointItems = new();
    private List<PointItem> _firstPassesPointItems = new();

    [RelayCommand]
    private void VerifyNode()
    {
        foreach (var pointItem in Nodes)
        {
            pointItem.Status = s节点状态.未验证;
            foreach (var connectorItem in pointItem.Output)
            {
                connectorItem.InputObject = null;
            }

            foreach (var connectorItem in pointItem.Input)
            {
                if (!connectorItem.IsSelf)
                {
                    connectorItem.InputObject = null;
                }
            }
        }

        _firstVerifyPointItems = new List<PointItem>();
        _firstPassesPointItems = new List<PointItem>();
        _firstVerifyPointItems.Add(Nodes[0]);
        _firstPassesPointItems.Add(Nodes[0]);
        Nodes[0].Status = s节点状态.已验证;
        var connectionItem = Connections.FirstOrDefault((e) => e.Source == Nodes[0].Output[0]);
        if (connectionItem == null)
        {
            return;
        }

        var firstNodes = connectionItem.Target.Source;
        try
        {
            ParsePointItem(firstNodes, false, true);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    private void ToFirstVerify(bool notRealTime = false)
    {
        foreach (var pointItem in Nodes)
        {
            pointItem.Status = s节点状态.未验证;
        }

        new Dictionary<ConnectionItem, object>();
        _firstVerifyPointItems = new List<PointItem>();
        _firstPassesPointItems = new List<PointItem>();
        _firstVerifyPointItems.Add(Nodes[0]);
        _firstPassesPointItems.Add(Nodes[0]);
        Nodes[0].Status = notRealTime ? s节点状态.已验证 : s节点状态.初步验证;

        var connectionItem = Connections.FirstOrDefault((e) => e.Source == Nodes[0].Output[0]);
        if (connectionItem == null)
        {
            return;
        }

        var firstNodes = connectionItem.Target.Source;
        ParsePointItem(firstNodes, false);
    }

    public void ParsePointItem(PointItem nowPointItem, bool onlyForward, bool notRealTime = false)
    {
        bool valid = true;
        lock (_firstVerifyPointItems)
        {
            if (_firstVerifyPointItems.Contains(nowPointItem))
            {
                return;
            }
        }

        if (_firstVerifyPointItems.Contains(nowPointItem))
        {
            //如果包含则证明源节点已被解析
            DateTime beforeDT = System.DateTime.Now;

            while (_firstVerifyPointItems.Contains(nowPointItem) && !_firstPassesPointItems.Contains(nowPointItem))
            {
                DateTime afterDT = System.DateTime.Now;
                if (afterDT.Subtract(beforeDT).Seconds >= 5)
                {
                    break;
                }
            }
        }

        _firstVerifyPointItems.Add(nowPointItem);
        List<Task> sourceDataTask = new();
        foreach (var connectorItem in nowPointItem.Input)
        {
            if (!connectorItem.IsConnected)
            {
                if (connectorItem.Type.FullName != "PluginCore.NodeConnectorClass")
                {
                    //当前节点有一个输入参数不存在,验证失败
                    if (!connectorItem.IsSelf)
                    {
                        valid = false;
                        break;
                    }
                }
                else
                {
                    connectorItem.IsNotUsed = true;
                }
            }
            else if (connectorItem.Type.FullName == "PluginCore.NodeConnectorClass")
            {
                connectorItem.IsNotUsed = false;
            }

            //这是连接当前节点的节点
            var connectionItem = Connections.Where((e) => e.Target == connectorItem).ToList();
            foreach (var item in connectionItem)
            {
                var sourceSource = item.Source.Source;
                lock (_firstVerifyPointItems)
                {
                    if (_firstVerifyPointItems.Contains(sourceSource))
                    {
                        //如果包含则证明源节点已被解析
                        DateTime beforeDT = System.DateTime.Now;

                        while (_firstVerifyPointItems.Contains(sourceSource) &&
                               !_firstPassesPointItems.Contains(sourceSource))
                        {
                            DateTime afterDT = System.DateTime.Now;
                            if (afterDT.Subtract(beforeDT).Seconds >= 5)
                            {
                                break;
                            }
                        }

                        continue;
                    }

                    var task = new Task(() =>
                    {
                        ParsePointItem(sourceSource, true, notRealTime);
                    });
                    task.Start();
                    sourceDataTask.Add(task);
                }


                //源解析完成
            }
        } //源数据全部生成

        Task.WaitAll(sourceDataTask.ToArray(), TimeSpan.FromSeconds(10));


        if (!valid)
        {
            nowPointItem.Status = s节点状态.错误;
            _firstPassesPointItems.Add(nowPointItem);
            return;
        }


        if (notRealTime)
        {
            try
            {
                if (nowPointItem.Plugin == "Kitopia")
                {
                    switch (nowPointItem.MerthodName)
                    {
                        case "判断":
                        {
                            if (nowPointItem.Input[1].InputObject is true)
                            {
                                nowPointItem.Output[0].InputObject = "当前流";
                                nowPointItem.Output[1].IsNotUsed = true;
                                nowPointItem.Output[1].InputObject = "未使用的流";
                            }
                            else
                            {
                                nowPointItem.Output[1].InputObject = "当前流";
                                nowPointItem.Output[0].InputObject = "未使用的流";
                                nowPointItem.Output[0].IsNotUsed = true;
                            }

                            break;
                        }
                        case "相等":
                        {
                            nowPointItem.Output[0].InputObject = nowPointItem.Input[1].InputObject
                                .Equals(nowPointItem.Input[2].InputObject);
                            var nextNode = Connections.Where((e) => e.Source == nowPointItem.Output[0]).ToList();
                            foreach (var item in nextNode)
                            {
                                item.Target.InputObject = nowPointItem.Output[0].InputObject;
                            }

                            break;
                        }
                        default:
                        {
                            var userInputConnector = nowPointItem.Input.FirstOrDefault();
                            if (userInputConnector is null)
                            {
                                throw new NullReferenceException();
                            }

                            var userInputData = userInputConnector.InputObject;
                            if (nowPointItem.MerthodName == "System.Int32")
                            {
                                userInputData = int.Parse(userInputData.ToString());
                            }

                            if (userInputData is null or "")
                            {
                                //这是用户自输入控件没有数据直接抛出异常
                                throw new NullReferenceException();
                            }

                            nowPointItem.Output[0].InputObject = userInputData;
                            var nextNode = Connections.Where((e) => e.Source == nowPointItem.Output[0]).ToList();
                            foreach (var item in nextNode)
                            {
                                item.Target.InputObject = userInputData;
                            }

                            break;
                        }
                    }
                }
                else
                {
                    var plugin = PluginManager.EnablePlugin[nowPointItem.Plugin];
                    var methodInfo = plugin.GetMethodInfos()[nowPointItem.MerthodName];
                    List<object> list = new();
                    int index = 1;
                    foreach (var parameterInfo in methodInfo.GetParameters())
                    {
                        if (parameterInfo.ParameterType.GetCustomAttribute(typeof(AutoUnbox)) is not null)
                        {
                            var autoUnboxIndex = nowPointItem.Input[index].AutoUnboxIndex;
                            List<object> parameterList = new List<object>();
                            List<Type> parameterTypesList = new();
                            while (nowPointItem.Input.Count >= index &&
                                   nowPointItem.Input[index].AutoUnboxIndex == autoUnboxIndex)
                            {
                                parameterList.Add(nowPointItem.Input[index].InputObject);
                                parameterTypesList.Add(nowPointItem.Input[index].InputObject.GetType());
                                index++;
                            }

                            var instance = parameterInfo.ParameterType.GetConstructor(parameterTypesList.ToArray())
                                .Invoke(parameterList.ToArray());
                            list.Add(instance);
                            continue;
                        }
                        else
                        {
                            list.Add(nowPointItem.Input[index].InputObject);
                        }

                        index++;
                    }

                    var invoke = methodInfo.Invoke(plugin.ServiceProvider.GetService(methodInfo.DeclaringType),
                        list.ToArray());

                    if (methodInfo.ReturnParameter.ParameterType.GetCustomAttribute(typeof(AutoUnbox)) is not null)
                    {
                        var type = methodInfo.ReturnParameter.ParameterType;
                        foreach (var memberInfo in type.GetProperties())
                        {
                            foreach (var connectorItem in nowPointItem.Output)
                            {
                                if (connectorItem.Type == memberInfo.PropertyType)
                                {
                                    var value = invoke.GetPropertyValue<object>(memberInfo.Name);
                                    connectorItem.InputObject = value;
                                    var nextNode = Connections.Where((e) => e.Source == connectorItem).ToList();
                                    foreach (var item in nextNode)
                                    {
                                        item.Target.InputObject = value;
                                    }

                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (nowPointItem.Output.Any())
                        {
                            nowPointItem.Output[0].InputObject = invoke;
                            var nextNode = Connections.Where((e) => e.Source == nowPointItem.Output[0]).ToList();
                            foreach (var item in nextNode)
                            {
                                item.Target.InputObject = invoke;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                valid = false;
                goto finnish;
            }
        }

        if (!onlyForward)
        {
            foreach (var outputConnector in nowPointItem.Output)
            {
                var thisToNextConnections = Connections.Where((e) => e.Source == outputConnector).ToList();
                foreach (var thisToNextConnection in thisToNextConnections)
                {
                    var nextPointItem = thisToNextConnection.Target.Source;

                    if (_firstVerifyPointItems.Contains(nextPointItem))
                    {
                        //如果包含则证明子节点已被解析
                        continue;
                    }

                    if (!outputConnector.IsNotUsed)
                    {
                        ThreadPool.QueueUserWorkItem((e) =>
                        {
                            ParsePointItem(nextPointItem, false, notRealTime);
                        });
                    }
                }
            }
        }

        finnish:
        if (valid)
        {
            nowPointItem.Status = notRealTime ? s节点状态.已验证 : s节点状态.初步验证;
        }

        _firstPassesPointItems.Add(nowPointItem);
    }
}