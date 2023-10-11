#region

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using Core.SDKs.Services.Plugin;
using Core.ViewModel.TaskEditor;
using log4net;
using Newtonsoft.Json;
using PluginCore.Attribute;
using Vanara.Extensions.Reflection;

#endregion

namespace Core.SDKs.CustomScenario;

public partial class CustomScenario : ObservableRecipient
{
    private static readonly ILog Log = LogManager.GetLogger(nameof(CustomScenario));

    [JsonIgnore] [ObservableProperty] [NotifyPropertyChangedRecipients]
    private bool _isRunning = false;

    /// <summary>
    ///     手动执行
    /// </summary>
    [JsonIgnore] [ObservableProperty] [NotifyPropertyChangedRecipients]
    private bool executionManual = true;

    [JsonIgnore] [ObservableProperty] [NotifyPropertyChangedRecipients]
    private ObservableCollection<string> keys = new();

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

    [JsonIgnore] [ObservableProperty] [NotifyPropertyChangedRecipients]
    private string _name = "任务";

    [JsonIgnore] [ObservableProperty] [NotifyPropertyChangedRecipients]
    private string _description = "";

    [JsonIgnore] [ObservableProperty] [NotifyPropertyChangedRecipients]
    private List<CustomScenarioInvoke> _autoTriggerType = new();

    private readonly Dictionary<PointItem, Thread?> _tasks = new();

    public void Run(bool realTime = false)
    {
        StartRun(!realTime);
    }

    private void StartRun(bool notRealTime)
    {
        if (IsRunning)
        {
            return;
        }

        if (notRealTime)
        {
            IsRunning = true;
        }

        foreach (var task in _tasks)
        {
            task.Value?.Interrupt();
        }

        _tasks.Clear();
        if (notRealTime)
        {
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
        }

        for (var i = nodes.Count - 1; i >= 1; i--)
        {
            var toRemove = nodes[i].Input.All(connectorItem => !connectorItem.IsConnected);

            if (nodes[i].Output.Any(connectorItem => connectorItem.IsConnected))
            {
                toRemove = false;
            }

            if (toRemove)
            {
                nodes[i].Status = s节点状态.未验证;
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
            ParsePointItem(firstNodes, false, notRealTime);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        //监听任务是否结束
        if (notRealTime)
        {
            new Task(() =>
            {
                while (true)
                {
                    Thread.Sleep(100);
                    var f = true;
                    foreach (var (_, value) in _tasks)
                    {
                        if (value is null)
                        {
                            continue;
                        }

                        if (!value.IsAlive)
                        {
                            continue;
                        }

                        f = false;
                        break;
                    }

                    if (!f)
                    {
                        continue;
                    }

                    IsRunning = false;
                    Log.Debug($"场景运行完成:{Name}");
                    break;
                }
            }).Start();
        }
    }

    public void Stop()
    {
        foreach (var task in _tasks)
        {
            task.Value?.Interrupt();
        }
    }

    private void ParsePointItem(PointItem nowPointItem, bool onlyForward, bool notRealTime)
    {
        Log.Debug($"解析节点:{nowPointItem.Title}");
        var valid = true;
        List<Thread> sourceDataTask = new();

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

            foreach (var sourceSource in connectionItem.Select(item => item.Source.Source))
            {
                lock (_tasks)
                {
                    if (_tasks.TryGetValue(sourceSource, out var task1))
                    {
                        if (task1 is not null)
                        {
                            sourceDataTask.Add(task1);
                        }
                    }
                    else
                    {
                        var task = new Thread(() =>
                        {
                            ParsePointItem(sourceSource, true, notRealTime);
                        });
                        task.Start();
                        // Log.Debug(sourceSource.Title);
                        _tasks.Add(sourceSource, task);
                        sourceDataTask.Add(task);
                    }
                }
            }
        }
        //源数据全部生成

        foreach (var thread in sourceDataTask)
        {
            thread.Join();
        }
        //这是连接当前节点的节点

        foreach (var connectorItem in nowPointItem.Input)
        {
            //这是连接当前节点的节点
            var connectionItem = connections.Where((e) => e.Target == connectorItem).ToList();

            foreach (var sourceSource in connectionItem.Select(item => item.Source.Source))
            {
                if (sourceSource.Status == s节点状态.错误)
                {
                    valid = false;
                }
            }
        }

        //源数据全部生成
        if (!valid)
        {
            goto finnish;
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
                            if (nowPointItem.Input[1].InputObject is null)
                            {
                                nowPointItem.Output[0].InputObject = false;
                            }
                            else if (nowPointItem.Input[2].InputObject is null)
                            {
                                nowPointItem.Output[0].InputObject = false;
                            }
                            else
                            {
                                nowPointItem.Output[0].InputObject =
                                    nowPointItem.Input[1].InputObject!.Equals(nowPointItem.Input[2].InputObject);
                            }

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
                            if (userInputData is null or "")
                            {
                                throw new NullReferenceException();
                            }

                            if (nowPointItem.MerthodName == "System.Int32")
                            {
                                userInputData = int.Parse(userInputData.ToString()!);
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
                    var index = 1;
                    foreach (var parameterInfo in methodInfo.GetParameters())
                    {
                        if (parameterInfo.ParameterType.GetCustomAttribute(typeof(AutoUnbox)) is not null)
                        {
                            var autoUnboxIndex = nowPointItem.Input[index].AutoUnboxIndex;
                            var parameterList = new List<object>();
                            List<Type> parameterTypesList = new();
                            while (nowPointItem.Input.Count >= index &&
                                   nowPointItem.Input[index].AutoUnboxIndex == autoUnboxIndex)
                            {
                                var item = nowPointItem.Input[index].InputObject;
                                if (item != null)
                                {
                                    parameterList.Add(item);
                                    parameterTypesList.Add(item.GetType());
                                }
                                else
                                {
                                    valid = false;
                                    goto finnish;
                                }

                                index++;
                            }

                            var instance = parameterInfo.ParameterType.GetConstructor(parameterTypesList.ToArray())
                                ?.Invoke(parameterList.ToArray());
                            if (instance != null)
                            {
                                list.Add(instance);
                            }
                            else
                            {
                                valid = false;
                                goto finnish;
                            }

                            continue;
                        }

                        var inputObject = nowPointItem.Input[index].InputObject;
                        if (inputObject != null)
                        {
                            list.Add(inputObject);
                        }
                        else
                        {
                            valid = false;
                            goto finnish;
                        }

                        index++;
                    }

                    var invoke = methodInfo.Invoke(plugin.ServiceProvider!.GetService(methodInfo.DeclaringType!),
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
                foreach (var nextPointItem in thisToNextConnections
                             .Select(thisToNextConnection => thisToNextConnection.Target.Source)
                             .Where(_ => !outputConnector.IsNotUsed))
                {
                    lock (_tasks)
                    {
                        if (_tasks.ContainsKey(nextPointItem))
                        {
                            return;
                        }

                        var task = new Thread(() =>
                        {
                            ParsePointItem(nextPointItem, false, notRealTime);
                        });
                        task.Start();
                        _tasks.Add(nextPointItem, task);
                    }
                }
            }
        }

        finnish:
        if (valid)
        {
            nowPointItem.Status = notRealTime ? s节点状态.已验证 : s节点状态.初步验证;
            Log.Debug($"解析节点完成:{nowPointItem.Title}");
        }
        else
        {
            nowPointItem.Status = s节点状态.错误;
            Log.Debug($"解析节点失败:{nowPointItem.Title}");
        }
    }

    [OnDeserializing]
    // ReSharper disable once UnusedMember.Local
    // ReSharper disable once UnusedParameter.Local
    private void OnDeserializing(StreamingContext context) //反序列化时hotkeys的默认值会被添加,需要先清空
    {
    }
}