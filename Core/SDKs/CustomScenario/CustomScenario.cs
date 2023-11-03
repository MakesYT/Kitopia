#region

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Core.SDKs.CustomType;
using Core.SDKs.Services.Plugin;
using Core.SDKs.Tools;
using Core.ViewModel.TaskEditor;
using log4net;
using Newtonsoft.Json;
using PluginCore.Attribute;
using Vanara.Extensions.Reflection;

#endregion

namespace Core.SDKs.CustomScenario;

public partial class CustomScenario : ObservableRecipient, IDisposable
{
    private static readonly ILog Log = LogManager.GetLogger(nameof(CustomScenario));
    [JsonIgnore] [ObservableProperty] private List<CustomScenarioInvoke> _autoTriggerType = new();
    private CancellationTokenSource _cancellationTokenSource = new();

    [JsonIgnore] [ObservableProperty] private string _description = "";

    private Dictionary<PointItem, Thread?> _initTasks = new();

    [JsonIgnore] [ObservableProperty] private bool _isRunning = false;

    [JsonIgnore] [ObservableProperty] private string _name = "任务";
    private Dictionary<PointItem, Thread?> _tickTasks = new();
    private TickUtil? _tickUtil;

    /// <summary>
    ///     手动执行
    /// </summary>
    [JsonIgnore] [ObservableProperty] private bool executionManual = true;

    private bool InTick = false;

    [JsonIgnore] [ObservableProperty] private ObservableCollection<string> keys = new();

    private TickUtil? tick;
    [JsonIgnore] [ObservableProperty] private double tickIntervalSecond = 5;
    [JsonIgnore] [ObservableProperty] private ObservableDictionary<string, object> values = new() { { "1", "2" } };

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

    public void Dispose()
    {
        _cancellationTokenSource.Dispose();
    }

    partial void OnNameChanged(string value)
    {
        WeakReferenceMessenger.Default.Send(new CustomScenarioChangeMsg()
            { Type = 1, Name = "Name", CustomScenario = this });
    }

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

        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();
        _cancellationTokenSource = new CancellationTokenSource();

        if (notRealTime)
        {
            IsRunning = true;
        }


        foreach (var task in _initTasks)
        {
            task.Value?.Join();
        }

        foreach (var task in _tickTasks)
        {
            task.Value?.Join();
        }

        _initTasks.Clear();
        _tickTasks.Clear();
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

        _initTasks.Add(nodes[0], null);
        nodes[0].Status = s节点状态.已验证;
        var connectionItem = connections.FirstOrDefault((e) => e.Source == nodes[0].Output[0]);
        if (connectionItem == null)
        {
            if (notRealTime)
            {
                IsRunning = false;
                Log.Debug($"场景运行完成:{Name}");
            }

            return;
        }

        var firstNodes = connectionItem.Target.Source;
        try
        {
            _initTasks.Add(firstNodes, null);
            ParsePointItem(_initTasks, firstNodes, false, notRealTime, _cancellationTokenSource.Token);
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
                    foreach (var (_, value) in _initTasks)
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

                    if (!notRealTime)
                    {
                        return;
                    }

                    var connectionItem = connections.FirstOrDefault((e) => e.Source == nodes[1].Output[0]);
                    if (connectionItem == null)
                    {
                        //当没有tick时直接结束
                        if (notRealTime)
                        {
                            _cancellationTokenSource.Cancel();
                        }

                        IsRunning = false;
                        Log.Debug($"场景运行完成:{Name}");
                        break;
                    }

                    Log.Debug($"场景进入Tick:{Name}");
                    try
                    {
                        _tickUtil = new TickUtil(1000, (uint)(tickIntervalSecond * 1000 * 1000), 1, TickMethod);
                        _tickUtil.Open();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }

                    break;
                }
            }).Start();
        }
    }

    private void TickMethod(object sender, long JumpPeriod, long interval)
    {
        if (InTick)
        {
            return;
        }

        var connectionItem = connections.FirstOrDefault((e) => e.Source == nodes[1].Output[0]);
        var firstNodes = connectionItem.Target.Source;
        nodes[1].Status = s节点状态.已验证;
        _tickTasks.Add(nodes[0], null);
        _tickTasks.Add(nodes[1], null);
        _tickTasks.Add(firstNodes, null);
        ParsePointItem(_tickTasks, firstNodes, false, true, _cancellationTokenSource.Token);

        while (true)
        {
            if (_cancellationTokenSource.Token.IsCancellationRequested)
            {
                InTick = false;
                break;
            }

            Thread.Sleep(100);
            var f = true;
            foreach (var (_, value) in _tickTasks)
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

            //tick完成一次
            InTick = false;
            _tickTasks.Clear();
            break;
        }
    }

    public void Stop()
    {
        _tickUtil?.Dispose();
        try
        {
            _cancellationTokenSource.Cancel();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        foreach (var task in _initTasks)
        {
            task.Value?.Join();
        }

        foreach (var task in _tickTasks)
        {
            task.Value?.Join();
        }

        _initTasks.Clear();
        _tickTasks.Clear();
        IsRunning = false;
        Log.Debug($"场景运行完成:{Name}");
    }

    private void MakeSourcePointState(ConnectorItem targetConnectorItem, PointItem pointItem)
    {
        foreach (var connectionItem in connections.Where(e => e.Target == targetConnectorItem))
        {
            if (connectionItem.Source.Source == pointItem)
            {
                connectionItem.Source.Source.Status = s节点状态.已验证;
            }
            else
            {
                connectionItem.Source.Source.Status = s节点状态.未验证;
            }
        }
    }

    private void ParsePointItem(Dictionary<PointItem, Thread?> threads, PointItem nowPointItem, bool onlyForward,
        bool notRealTime,
        CancellationToken cancellationToken)
    {
        Log.Debug($"解析节点:{nowPointItem.Title}");
        var valid = true;
        List<Thread> sourceDataTask = new();
        var indexOf = nodes.IndexOf(nowPointItem);
        if (indexOf is 0 or 1)
        {
            goto finnish;
        }

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
            foreach (var sourceSource in connectorItem.GetSourceOrNextPointItems(connections))
            {
                lock (threads)
                {
                    if (threads.TryGetValue(sourceSource, out var task1))
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
                            ParsePointItem(threads, sourceSource, true, notRealTime, cancellationToken);
                        });

                        // Log.Debug(sourceSource.Title);
                        threads.Add(sourceSource, task);
                        sourceDataTask.Add(task);
                        task.Start();
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
        if (cancellationToken.IsCancellationRequested)
        {
            return;
        }

        foreach (var connectorItem in nowPointItem.Input)
        {
            foreach (var sourceSource in connectorItem.GetSourceOrNextPointItems(connections))
            {
                if (sourceSource.Status == s节点状态.错误)
                {
                    valid = false;
                }
            }
        }


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
                        case "一对N":
                        {
                            for (var i = 0; i < nowPointItem.Output.Count; i++)
                            {
                                nowPointItem.Output[i].InputObject = $"流{i + 1}";
                            }

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

                            foreach (var item in nowPointItem.Output[0].GetSourceOrNextConnectorItems(connections))
                            {
                                item.InputObject = nowPointItem.Output[0].InputObject;
                                MakeSourcePointState(item, nowPointItem);
                            }

                            break;
                        }
                        case "valueSet":
                        {
                            if (Values.ContainsKey(nowPointItem.ValueRef!))
                            {
                                Values.SetValueWithoutNotify(nowPointItem.ValueRef!,
                                    nowPointItem.Input[1].InputObject!);
                            }

                            break;
                        }
                        case "valueGet":
                        {
                            if (Values.ContainsKey(nowPointItem.ValueRef!))
                            {
                                foreach (var item in nowPointItem.Output[0].GetSourceOrNextConnectorItems(connections))
                                {
                                    item.InputObject = Values[nowPointItem.ValueRef!];
                                    MakeSourcePointState(item, nowPointItem);
                                }
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
                            foreach (var item in nowPointItem.Output[0].GetSourceOrNextConnectorItems(connections))
                            {
                                item.InputObject = userInputData;
                                MakeSourcePointState(item, nowPointItem);
                            }

                            break;
                        }
                    }
                }
                else
                {
                    var plugin = PluginManager.EnablePlugin[nowPointItem.Plugin];
                    var customScenarioNodeMethod = PluginOverall.CustomScenarioNodeMethods[nowPointItem.Plugin];

                    var methodInfo = customScenarioNodeMethod[nowPointItem.MerthodName].Item1;
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

                        if (index == nowPointItem.Input.Count)
                        {
                            list.Add(cancellationToken);
                            break;
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
                                    foreach (var item in connectorItem.GetSourceOrNextConnectorItems(connections))
                                    {
                                        item.InputObject = value;
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
                            foreach (var item in nowPointItem.Output[0].GetSourceOrNextConnectorItems(connections))
                            {
                                item.InputObject = invoke;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Debug(e);
                valid = false;
                goto finnish;
            }
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return;
        }

        if (!onlyForward)
        {
            foreach (var outputConnector in nowPointItem.Output)
            {
                foreach (var nextPointItem in outputConnector.GetSourceOrNextPointItems(connections)
                             .Where(_ => !outputConnector.IsNotUsed))
                {
                    lock (threads)
                    {
                        if (threads.ContainsKey(nextPointItem))
                        {
                            return;
                        }

                        var task = new Thread(() =>
                        {
                            ParsePointItem(threads, nextPointItem, false, notRealTime, cancellationToken);
                        });

                        threads.Add(nextPointItem, task);
                        task.Start();
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