﻿#region

using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Core.SDKs.CustomType;
using Core.SDKs.HotKey;
using Core.SDKs.Services;
using Core.SDKs.Tools;
using log4net;
using PluginCore;

#endregion

namespace Core.SDKs.CustomScenario;

public partial class CustomScenario : ObservableRecipient
{
    private static readonly ILog Log = LogManager.GetLogger(nameof(CustomScenario));

    [JsonIgnore] [ObservableProperty] private ObservableCollection<string> _autoTriggers = new();
    private CancellationTokenSource _cancellationTokenSource = new();

    [JsonIgnore] [ObservableProperty] private string _description = "";
    private Dictionary<ScenarioMethodNode, Thread?> _initTasks = new();

    [JsonIgnore] [ObservableProperty] private bool _isRunning = false;

    [JsonIgnore] [ObservableProperty] private DateTime _lastRun;

    [JsonIgnore] [ObservableProperty] private string _name = "情景";
    public Dictionary<string, int> PluginUsedCount = new();
    private Dictionary<ScenarioMethodNode, Thread?> _tickTasks = new();
    private TickUtil? _tickUtil;

    /// <summary>
    ///     手动执行
    /// </summary>
    [JsonIgnore] [ObservableProperty] private bool executionManual = true;

    [JsonIgnore] [ObservableProperty] private bool hasInit = true;
    [JsonIgnore] [ObservableProperty] private string? initError;
    [JsonIgnore] [ObservableProperty] private ObservableDictionary<string, object?> inputValue = new();

    private bool InTick;

    [JsonIgnore] [ObservableProperty] private bool isHaveInputValue = false;

    [JsonIgnore] [ObservableProperty] private ObservableCollection<string> keys = new();


    [JsonIgnore] [ObservableProperty] private double? tickIntervalSecond = 5;

    [JsonIgnore] [ObservableProperty] private ObservableDictionary<string, object> values = new();

    //ActiveHotKey
    [JsonIgnore] [ObservableProperty] public HotKeyModel runHotKey = new()
    {
        MainName = "Kitopia情景", Name = "情景", IsUsable = false, IsSelectCtrl = false, IsSelectAlt = false,
        IsSelectWin = false,
        IsSelectShift = false, SelectKey = EKey.未设置,
    };

    [JsonIgnore] [ObservableProperty] public HotKeyModel stopHotKey = new()
    {
        MainName = "Kitopia情景", Name = "情景", IsUsable = false, IsSelectCtrl = false, IsSelectAlt = false,
        IsSelectWin = false,
        IsSelectShift = false, SelectKey = EKey.未设置,
    };

    public CustomScenario()
    {
        PropertyChanged += PropertyChangedEventHandler();

        runHotKey.Name = $"{UUID}_激活快捷键";
        stopHotKey.Name = $"{UUID}_停止快捷键";


        InputValue.CollectionChanged += OnInputValueOnCollectionChanged;
    }


    void OnInputValueOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs args)
    {
        var outputs = nodes.First()
            .Output;
        for (var i = outputs.Count - 1; i >= 1; i--)
        {
            outputs.RemoveAt(i);
        }

        if (sender is ObservableDictionary<string, object> dictionary)
        {
            foreach (var (key, value) in dictionary)
            {
                outputs.Add(new()
                {
                    IsOut = true,
                    Source = nodes.First(),
                    Type = value.GetType(),
                    TypeName = ScenarioMethodI18nTool.GetI18N(value.GetType()
                        .FullName),
                    Title = key
                });
            }
        }
    }

    private PropertyChangedEventHandler? PropertyChangedEventHandler()
    {
        return (e, s) =>
        {
            if (s.PropertyName == nameof(IsRunning))
            {
                return;
            }

            WeakReferenceMessenger.Default.Send(new CustomScenarioChangeMsg()
                { Type = 1, Name = nameof(e), CustomScenario = this });
        };
    }

    public string UUID { get; set; } = Guid.NewGuid()
        .ToString();

    public ObservableCollection<ScenarioMethodNode> nodes { get; set; } = new();

    public ObservableCollection<ConnectionItem> connections { get; set; } = new();

    public void Dispose()
    {
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();
        _tickTasks = null;
        _initTasks = null;
        nodes.Clear();
        //Log.Debug(Name + " Dispose");
    }

    partial void OnTickIntervalSecondChanged(double? oldValue)
    {
        if (oldValue is null)
        {
            TickIntervalSecond = 0.1;
        }
    }


    public void Run(bool realTime = false, bool onExit = false, params object[] inputValues)
    {
        if (IsHaveInputValue)
        {
            if (inputValues.Length != InputValue.Count)
            {
                return;
            }
        }

        StartRun(!realTime, onExit, inputValues);
    }

    private void StartRun(bool notRealTime, bool onExit = false, params object[] inputValues)
    {
        if (IsRunning || !HasInit)
        {
            return;
        }

        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();
        _cancellationTokenSource = new CancellationTokenSource();

        if (notRealTime)
        {
            IsRunning = true;
            LastRun = DateTime.Now;
            CustomScenarioManger.Save(this);
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

            for (var i = 0; i < inputValues.Length; i++)
            {
                nodes[0]
                    .Output[i + 1].InputObject = inputValues[i];
            }
        }

        for (var i = nodes.Count - 1; i >= 1; i--)
        {
            var toRemove = nodes[i]
                .Input.All(connectorItem => !connectorItem.IsConnected);

            if (nodes[i]
                .Output.Any(connectorItem => connectorItem.IsConnected))
            {
                toRemove = false;
            }

            if (toRemove)
            {
                nodes[i].Status = s节点状态.未验证;
            }
        }

        try
        {
            //_initTasks.Add( nodes[0], null);
            ParsePointItem(_initTasks, nodes[0], false, notRealTime, _cancellationTokenSource.Token);
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

                    if (_cancellationTokenSource.IsCancellationRequested)
                    {
                        return;
                    }

                    var connectionItem = connections.FirstOrDefault((e) => e.Source == nodes[1]
                        .Output[0]);
                    if (connectionItem == null || onExit)
                    {
                        //当没有tick时直接结束
                        if (notRealTime)
                        {
                            _cancellationTokenSource.Cancel();
                        }

                        IsRunning = false;
                        ((IToastService)ServiceManager.Services.GetService(typeof(IToastService))!).Show("情景",
                            $"情景{Name}运行完成");
                        Log.Debug($"情景运行完成:{Name}");
                        break;
                    }

                    ((IToastService)ServiceManager.Services.GetService(typeof(IToastService))!).Show("情景",
                        $"情景{Name}进入Tick");
                    Log.Debug($"情景进入Tick:{Name}");
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

        var nowPointItem = nodes[1];
        ParsePointItem(_tickTasks, nowPointItem, false, true, _cancellationTokenSource.Token);

        while (true)
        {
            if (_cancellationTokenSource.Token.IsCancellationRequested)
            {
                InTick = false;
                _tickUtil.Dispose();
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

    public void Stop(bool inTickError = false)
    {
        if (!IsRunning)
        {
            return;
        }


        try
        {
            _tickUtil?.Dispose();
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
        if (inTickError)
        {
            ((IToastService)ServiceManager.Services.GetService(typeof(IToastService))!)
                .Show("情景", $"情景{Name}由于出现错误被停止");
            Log.Debug($"情景{Name}由于出现错误被停止");
        }
        else
        {
            ((IToastService)ServiceManager.Services.GetService(typeof(IToastService))!).Show("情景", $"情景{Name}被用户停止");
            Log.Debug($"情景{Name}被用户停止");
        }
    }

    private void MakeSourcePointState(ConnectorItem targetConnectorItem, ScenarioMethodNode scenarioMethodNode)
    {
        foreach (var connectionItem in connections.Where(e => e.Target == targetConnectorItem))
        {
            if (connectionItem.Source.Source == scenarioMethodNode)
            {
                connectionItem.Source.Source.Status = s节点状态.已验证;
            }
            else
            {
                connectionItem.Source.Source.Status = s节点状态.未验证;
            }
        }
    }

    private void ParsePointItem(Dictionary<ScenarioMethodNode, Thread?> threads,
        ScenarioMethodNode nowScenarioMethodNode, bool onlyForward,
        bool notRealTime,
        CancellationToken cancellationToken)
    {
        Log.Debug($"解析节点:{nowScenarioMethodNode.Title}");
        var valid = true;
        List<Thread> sourceDataTask = new();

        foreach (var connectorItem in nowScenarioMethodNode.Input)
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

        foreach (var connectorItem in nowScenarioMethodNode.Input)
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
                nowScenarioMethodNode.Invoke(cancellationToken, connections, Values);
            }
            catch (Exception e)
            {
                Log.Debug(e);
                ((IToastService)ServiceManager.Services.GetService(typeof(IToastService))!).Show("情景",
                    $"情景{Name}出现错误\n{e.Message}");
                Task.Run(() => { Stop(true); });

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
            foreach (var outputConnector in nowScenarioMethodNode.Output)
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
            nowScenarioMethodNode.Status = notRealTime ? s节点状态.已验证 : s节点状态.初步验证;
            Log.Debug($"解析节点完成:{nowScenarioMethodNode.Title}");
        }
        else
        {
            nowScenarioMethodNode.Status = s节点状态.错误;
            Log.Debug($"解析节点失败:{nowScenarioMethodNode.Title}");
        }
    }


    public void OnDeserialized() //反序列化时hotkeys的默认值会被添加,需要先清空
    {
        PropertyChanged += PropertyChangedEventHandler();
        InputValue.CollectionChanged += OnInputValueOnCollectionChanged;
    }
}