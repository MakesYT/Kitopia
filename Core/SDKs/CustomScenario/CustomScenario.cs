using System.ComponentModel;
using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using Core.SDKs.Services.Plugin;
using Core.ViewModel.TaskEditor;
using PluginCore.Attribute;
using Vanara.Extensions.Reflection;

namespace Core.SDKs.CustomScenario;

public partial class CustomScenario : ObservableRecipient
{
    private List<Task> _tasks = new();

    [ObservableProperty] [NotifyPropertyChangedRecipients]
    public bool executionAuto = false;


    [ObservableProperty] [NotifyPropertyChangedRecipients]
    public List<object> autoScenarios = new List<object>();

    /// <summary>
    /// 手动执行
    /// </summary>
    [ObservableProperty] [NotifyPropertyChangedRecipients]
    public bool executionManual = true;


    [ObservableProperty] [NotifyPropertyChangedRecipients]
    public string key;

    /// <summary>
    /// 间隔指定时间执行
    /// </summary>
    [ObservableProperty] [NotifyPropertyChangedRecipients]
    public bool executionIntervalSpecifies = false;

    [ObservableProperty] [NotifyPropertyChangedRecipients]
    public List<TimeSpan> intervalSpecifiesTimeSpan;

    /// <summary>
    /// 指定时间执行
    /// </summary>
    [ObservableProperty] [NotifyPropertyChangedRecipients]
    public bool executionScheduleTime = false;

    [ObservableProperty] [NotifyPropertyChangedRecipients]
    public TimeSpan scheduleTime;

    public string? UUID
    {
        get;
        set;
    }

    public BindingList<PointItem> nodes
    {
        get;
        set;
    } = new();

    public BindingList<ConnectionItem> connections
    {
        get;
        set;
    } = new();

    [ObservableProperty] [NotifyPropertyChangedRecipients]
    public string _name = "任务";

    [ObservableProperty] [NotifyPropertyChangedRecipients]
    public string _description = "";

    private List<PointItem> _firstVerifyPointItems = new();
    private List<PointItem> _firstPassesPointItems = new();

    public void Run()
    {
        foreach (var task in _tasks)
        {
            task.Dispose();
        }

        _tasks.Clear();
        foreach (var pointItem in nodes)
        {
            //pointItem.Status = s节点状态.未验证;
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
        _firstVerifyPointItems.Add(nodes[0]);
        _firstPassesPointItems.Add(nodes[0]);
        //nodes[0].Status = s节点状态.已验证;
        var connectionItem = connections.FirstOrDefault((e) => e.Source == nodes[0].Output[0]);
        if (connectionItem == null)
        {
            return;
        }

        var firstNodes = connectionItem.Target.Source;
        try
        {
            ParsePointItem(firstNodes, false);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    public void Stop()
    {
        foreach (var task in _tasks)
        {
            task.Dispose();
        }
    }

    private void ParsePointItem(PointItem nowPointItem, bool onlyForward)
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
            var connectionItem = connections.Where((e) => e.Target == connectorItem).ToList();
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
                        ParsePointItem(sourceSource, true);
                    });
                    task.Start();
                    _tasks.Add(task);
                    sourceDataTask.Add(task);
                }


                //源解析完成
            }
        } //源数据全部生成

        Task.WaitAll(sourceDataTask.ToArray());


        if (!valid)
        {
            //nowPointItem.Status = s节点状态.错误;
            _firstPassesPointItems.Add(nowPointItem);
            return;
        }


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
                                nowPointItem.Output[0].IsNotUsed = false;
                                nowPointItem.Output[1].IsNotUsed = true;
                            }
                            else
                            {
                                nowPointItem.Output[0].IsNotUsed = true;
                                nowPointItem.Output[1].IsNotUsed = false;
                            }

                            break;
                        }
                        case "相等":
                        {
                            nowPointItem.Output[0].InputObject = nowPointItem.Input[1].InputObject
                                .Equals(nowPointItem.Input[2].InputObject);
                            var nextNode = connections.Where((e) => e.Source == nowPointItem.Output[0])
                                .ToList();
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
                            var nextNode = connections.Where((e) => e.Source == nowPointItem.Output[0])
                                .ToList();
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
                                    var nextNode = connections.Where((e) => e.Source == connectorItem)
                                        .ToList();
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
                            var nextNode = connections.Where((e) => e.Source == nowPointItem.Output[0])
                                .ToList();
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
                var thisToNextConnections = connections.Where((e) => e.Source == outputConnector).ToList();
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
                        var item = new Task(() =>
                        {
                            ParsePointItem(nextPointItem, false);
                        });
                        item.Start();
                        _tasks.Add(item);
                    }
                }
            }
        }

        finnish:


        _firstPassesPointItems.Add(nowPointItem);
    }
}