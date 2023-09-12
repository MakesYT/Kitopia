#region

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using Core.SDKs.Services.Plugin;
using Core.ViewModel.TaskEditor;
using log4net;
using PluginCore.Attribute;
using Vanara.Extensions.Reflection;

#endregion

namespace Core.SDKs.CustomScenario;

public partial class CustomScenario : ObservableRecipient
{
    private static readonly ILog Log = LogManager.GetLogger(nameof(CustomScenario));

    [ObservableProperty] [NotifyPropertyChangedRecipients]
    public bool executionAuto = false;


    [ObservableProperty] [NotifyPropertyChangedRecipients]
    public List<object> autoScenarios = new();

    /// <summary>
    ///     手动执行
    /// </summary>
    [ObservableProperty] [NotifyPropertyChangedRecipients]
    public bool executionManual = true;


    [ObservableProperty] [NotifyPropertyChangedRecipients]
    public ObservableCollection<string> keys = new();

    /// <summary>
    ///     间隔指定时间执行
    /// </summary>
    [ObservableProperty] [NotifyPropertyChangedRecipients]
    public bool executionIntervalSpecifies = false;

    [ObservableProperty] [NotifyPropertyChangedRecipients]
    public List<TimeSpan> intervalSpecifiesTimeSpan;

    /// <summary>
    ///     指定时间执行
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

    private Dictionary<PointItem, Task?> _tasks = new();

    public void Run()
    {
        foreach (var task in _tasks)
        {
            //TODO: 
            //task.Dispose();
        }

        _tasks.Clear();
        foreach (var pointItem in nodes)
        {
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

        _tasks.Add(nodes[0], null);
        nodes[0].Status = s节点状态.已验证;
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
            //TODO:
            //task.Dispose();
        }
    }

    private void ParsePointItem(PointItem nowPointItem, bool onlyForward)
    {
        Log.Debug($"解析节点:{nowPointItem.Title}");
        bool valid = true;
        List<Task> sourceDataTask = new();
        try
        {
            foreach (var connectorItem in nowPointItem.Input)
            {
                try
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
                }
                catch (Exception e)
                {
                    Log.Debug(e);
                }

                //这是连接当前节点的节点
                var connectionItem = connections.Where((e) => e.Target == connectorItem).ToList();

                foreach (var item in connectionItem)
                {
                    var sourceSource = item.Source.Source;
                    lock (_tasks)
                    {
                        if (_tasks.TryGetValue(sourceSource, out var task1))
                        {
                            if (task1 is not null)
                            {
                                sourceDataTask.Add(task1);
                            }

                            continue;
                        }
                        else
                        {
                            var task = new Task(() =>
                            {
                                ParsePointItem(sourceSource, true);
                            });
                            task.Start();
                            // Log.Debug(sourceSource.Title);
                            _tasks.Add(sourceSource, task);
                            sourceDataTask.Add(task);
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            Log.Error(e);
        } //源数据全部生成

        Task.WaitAll(sourceDataTask.ToArray());
        if (!valid)
        {
            nowPointItem.Status = s节点状态.错误;
            //_firstPassesPointItems.Add(nowPointItem);
            Log.Debug($"解析节点失败:{nowPointItem.Title}");
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
                                nowPointItem.Output[0].InputObject = "当前流";
                                nowPointItem.Output[0].IsNotUsed = false;
                                nowPointItem.Output[1].IsNotUsed = true;
                                nowPointItem.Output[1].InputObject = "未使用的流";
                            }
                            else
                            {
                                nowPointItem.Output[1].InputObject = "当前流";
                                nowPointItem.Output[0].InputObject = "未使用的流";
                                nowPointItem.Output[0].IsNotUsed = true;
                                nowPointItem.Output[1].IsNotUsed = false;
                            }

                            break;
                        }
                        case "一对二":
                        {
                            nowPointItem.Output[0].InputObject = "流1";
                            nowPointItem.Output[1].InputObject = "流2";
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
                Log.Debug(e.ToString());
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


                    if (!outputConnector.IsNotUsed)
                    {
                        lock (_tasks)
                        {
                            if (_tasks.ContainsKey(nextPointItem))
                            {
                                return;
                            }
                            else
                            {
                                var task = new Task(() =>
                                {
                                    ParsePointItem(nextPointItem, true);
                                });
                                task.Start();
                                _tasks.Add(nextPointItem, task);
                            }
                        }
                    }
                }
            }
        }

        finnish:
        if (valid)
        {
            //nowPointItem.Status = notRealTime ? s节点状态.已验证 : s节点状态.初步验证;
            Log.Debug($"解析节点完成:{nowPointItem.Title}");
        }
    }
}