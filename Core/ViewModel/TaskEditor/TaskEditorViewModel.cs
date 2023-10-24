﻿#region

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Core.SDKs.CustomScenario;
using Core.SDKs.Services;
using Core.SDKs.Services.Config;
using Core.SDKs.Services.Plugin;
using Core.SDKs.Tools;
using log4net;
using Newtonsoft.Json;
using PluginCore;

#endregion

namespace Core.ViewModel.TaskEditor;

public partial class TaskEditorViewModel : ObservableRecipient
{
    private static readonly ILog Log = LogManager.GetLogger(nameof(TaskEditorViewModel));

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCustomScenarioCommand))]
    [NotifyCanExecuteChangedFor(nameof(SaveAndQuitCustomScenarioCommand))]
    public bool _isModified = false;

    [ObservableProperty] private BindingList<BindingList<object>> _nodeMethods = new();

    [ObservableProperty] private CustomScenario _scenario = new CustomScenario() { IsActive = true };


    private Window _window;

    public TaskEditorViewModel()
    {
        WeakReferenceMessenger.Default.Register<CustomScenarioChangeMsg>(this, (a, e) =>
        {
            IsModified = true;
            if (e.Type == 1)
            {
                if (e.Name == "Name")
                {
                    e.CustomScenario.nodes[0].Title = e.CustomScenario.Name;
                }

                return;
            }

            if (Scenario.nodes.Contains(e.PointItem) || _nodeMethods.Any(a => a.Contains(e.PointItem)))
            {
                if (e.ConnectorItem is not null)
                {
                    if (e.PointItem.MerthodName == "一对N" && e.ConnectorItem.IsSelf)
                    {
                        if (e.PointItem.Output.Count == Convert.ToInt32((double)e.ConnectorItem.InputObject))
                        {
                            return;
                        }

                        while (e.PointItem.Output.Count != Convert.ToInt32((double)e.ConnectorItem.InputObject))
                        {
                            if (e.PointItem.Output.Count > Convert.ToInt32((double)e.ConnectorItem.InputObject))
                            {
                                var connectorItem = e.PointItem.Output[^1];
                                var connectionItems = Scenario.connections
                                    .Where((connectionItem) => connectionItem.Source == connectorItem).ToList();
                                foreach (var connectionItem in connectionItems)
                                {
                                    Scenario.connections.Remove(connectionItem);
                                    if (Scenario.connections.All(item => item.Source != connectionItem.Source))
                                    {
                                        connectionItem.Source.IsConnected = false;
                                    }

                                    if (Scenario.connections.All(item => item.Target != connectionItem.Target))
                                    {
                                        connectionItem.Target.IsConnected = false;
                                    }
                                }

                                e.PointItem.Output.Remove(connectorItem);
                            }
                            else
                            {
                                e.PointItem.Output.Add(new ConnectorItem()
                                {
                                    Source = e.PointItem,
                                    Type = typeof(NodeConnectorClass),
                                    Title = "流输出",
                                    IsOut = true,
                                    TypeName = "节点"
                                });
                            }
                        }
                    }
                }
            }
        });

        PendingConnection = new PendingConnectionViewModel(this);
        GetAllMethods();
        var nodify2 = new PointItem()
        {
            Title = "任务1"
        };
        nodify2.Output = new ObservableCollection<ConnectorItem>
        {
            new()
            {
                IsOut = true,
                Source = nodify2,
                Type = typeof(NodeConnectorClass),
                TypeName = BaseNodeMethodsGen.GetI18N(typeof(NodeConnectorClass).FullName),
                Title = "开始"
            }
        };
        Scenario.nodes.Add(nodify2);
        //nodeMethods.Add("new PointItem(){Title = \"Test\"}");
    }

    public object ContentPresenter
    {
        get;
        set;
    }

    public PendingConnectionViewModel PendingConnection
    {
        get;
    }


    [RelayCommand]
    private void AddNodes(PointItem pointItem)
    {
        IsModified = true;

        var item = new PointItem()
        {
            Title = pointItem.Title,
            Plugin = pointItem.Plugin,
            MerthodName = pointItem.MerthodName,
            Location = new Point(pointItem.Location.X, pointItem.Location.Y)
        };
        ObservableCollection<ConnectorItem> input = new();
        foreach (var connectorItem in pointItem.Input)
        {
            input.Add(new ConnectorItem()
            {
                Anchor = new Point(connectorItem.Anchor.X, connectorItem.Anchor.Y),
                Source = item,
                TypeName = connectorItem.TypeName,
                Title = connectorItem.Title,
                Type = connectorItem.Type,
                RealType = connectorItem.RealType,
                InputObject = connectorItem.InputObject,
                AutoUnboxIndex = connectorItem.AutoUnboxIndex,
                IsConnected = connectorItem.IsConnected,
                IsSelf = connectorItem.IsSelf,
                IsOut = connectorItem.IsOut
            });
        }

        ObservableCollection<ConnectorItem> output = new();
        foreach (var connectorItem in pointItem.Output)
        {
            var connectorItem1 = new ConnectorItem()
            {
                Anchor = new Point(connectorItem.Anchor.X, connectorItem.Anchor.Y),
                Source = item,
                Title = connectorItem.Title,
                TypeName = connectorItem.TypeName,
                RealType = connectorItem.RealType,
                AutoUnboxIndex = connectorItem.AutoUnboxIndex,
                Type = connectorItem.Type,
                IsConnected = connectorItem.IsConnected,
                IsOut = connectorItem.IsOut
            };
            if (connectorItem.Interfaces is { Count: > 0 })
            {
                List<string> interfaces = new();
                foreach (var connectorItemInterface in connectorItem.Interfaces)
                {
                    interfaces.Add(connectorItemInterface);
                }

                connectorItem1.Interfaces = interfaces;
            }

            output.Add(connectorItem1);
        }

        item.Input = input;
        item.Output = output;
        Scenario.nodes.Add(item);
    }

    [RelayCommand]
    private void DelNode(PointItem pointItem)
    {
        IsModified = true;
        if (Scenario.nodes.IndexOf(pointItem) == 0)
        {
            return;
        }

        var connectionItems = Scenario.connections
            .Where((e) => e.Source.Source == pointItem || e.Target.Source == pointItem).ToList();
        foreach (var connectionItem in connectionItems)
        {
            Scenario.connections.Remove(connectionItem);
            if (Scenario.connections.All(e => e.Source != connectionItem.Source))
            {
                connectionItem.Source.IsConnected = false;
            }

            if (Scenario.connections.All(e => e.Target != connectionItem.Target))
            {
                connectionItem.Target.IsConnected = false;
            }
        }

        Scenario.nodes.Remove(pointItem);
    }

    [RelayCommand]
    private void DelConnection(ConnectionItem connection)
    {
        IsModified = true;
        Scenario.connections.Remove(connection);
        if (Scenario.connections.All(e => e.Source != connection.Source))
        {
            connection.Source.IsConnected = false;
        }

        if (Scenario.connections.All(e => e.Target != connection.Target))
        {
            connection.Target.IsConnected = false;
        }

        IsModified = true;
        ToFirstVerify();
    }


    [RelayCommand(CanExecute = nameof(IsModified))]
    private void SaveCustomScenario()
    {
        CleanUnusedNode();

        IsModified = false;
        CustomScenarioManger.Save(Scenario);
    }

    [RelayCommand(CanExecute = nameof(IsModified))]
    private void SaveAndQuitCustomScenario(Window window)
    {
        CleanUnusedNode();
        ((IContentDialog)ServiceManager.Services!.GetService(typeof(IContentDialog))!).ShowDialogAsync(ContentPresenter,
            "保存并退出?", "是否确定保存并退出",
            () =>
            {
                IsModified = false;
                CustomScenarioManger.Save(Scenario);
                Application.Current.Dispatcher.BeginInvoke(() =>
                {
                    window.Close();
                });
            }, () =>
            {
            }
        );
    }

    [RelayCommand]
    private void CleanUnusedNode()
    {
        for (var i = Scenario.nodes.Count - 1; i >= 1; i--)
        {
            bool toRemove = true;

            foreach (var connectorItem in Scenario.nodes[i].Input)
            {
                if (connectorItem.IsConnected)
                {
                    toRemove = false;
                    break;
                }
            }

            if (!toRemove)
            {
                continue;
            }

            foreach (var connectorItem in Scenario.nodes[i].Output)
            {
                if (connectorItem.IsConnected)
                {
                    toRemove = false;
                    break;
                }
            }

            if (toRemove)
            {
                IsModified = true;
                Scenario.nodes.Remove(Scenario.nodes[i]);
            }
        }
    }

    [RelayCommand]
    private void DisconnectConnector(ConnectorItem connector)
    {
        var connections = Scenario.connections.Where((e) => e.Source == connector || e.Target == connector).ToList();
        for (var i = connections.Count - 1; i >= 0; i--)
        {
            var connection = connections[i];
            Scenario.connections.Remove(connection);
            if (Scenario.connections.All(e => e.Source != connection.Source))
            {
                connection.Source.IsConnected = false;
            }

            if (Scenario.connections.All(e => e.Target != connection.Target))
            {
                connection.Target.IsConnected = false;
            }

            IsModified = true;
        }

        ToFirstVerify();
    }

    private void GetAllMethods()
    {
        BaseNodeMethodsGen.GenBaseNodeMethods(NodeMethods);

        foreach (var customScenarioNodeMethod in PluginOverall.CustomScenarioNodeMethods)
        {
            var methods = new BindingList<object>();
            foreach (var keyValuePair in customScenarioNodeMethod.Value)
            {
                methods.Add(keyValuePair.Value.Item2);
            }

            NodeMethods.Add(methods);
        }
    }

    public void Load(CustomScenario customScenario)
    {
        Scenario = customScenario;
    }

    [RelayCommand]
    private void Load(object window)
    {
        _window = (Window)window;
    }

    [RelayCommand]
    public void ReloadScenario(CancelEventArgs e)
    {
        if (IsModified)
        {
            e.Cancel = true;
            ((IToastService)ServiceManager.Services!.GetService(typeof(IToastService))!).ShowMessageBoxW("不保存退出?",
                "是否确定不保存退出",
                new ShowMessageContent("取消", "不保存", "保存并退出", () =>
                {
                    IsModified = false;
                    CustomScenarioManger.Save(Scenario);
                    _window.Close();
                }, () =>
                {
                    IsModified = false;
                    CustomScenarioManger.Reload(Scenario);
                    _window.Close();
                }, () =>
                {
                    e.Cancel = true;
                }));
        }
    }

    public void Connect(ConnectorItem source, ConnectorItem target)
    {
        if (source.IsConnected)
        {
            if (Scenario.connections
                .Any(e => e.Source == source && e.Target == target))
            {
                return;
            }
        }

        if (source.Type.FullName == "PluginCore.NodeConnectorClass")
        {
            if (source.IsConnected)
            {
                var connectionsToRemove = Scenario.connections
                    .Where(e => e.Source == source)
                    .ToList();

                foreach (var connection in connectionsToRemove)
                {
                    connection.Source.IsConnected = false;


                    Scenario.connections.Remove(connection);
                    if (Scenario.connections.All(e => e.Target != connection.Target))
                    {
                        connection.Target.IsConnected = false;
                    }
                }
            }
        }

        IsModified = true;
        Scenario.connections.Add(new ConnectionItem(source, target));
        ToFirstVerify();
        //OnPropertyChanged(nameof(Connections));
    }

    #region 自定义关键词

    [NotifyCanExecuteChangedFor(nameof(DelKeyCommand))] [ObservableProperty]
    private int _isSelected = -1;


    private bool CanDel => IsSelected > -1;

    [RelayCommand(CanExecute = nameof(CanDel))]
    private void DelKey(string key)
    {
        Scenario.Keys.Remove(key);
        IsModified = true;
    }

    [NotifyPropertyChangedFor(nameof(CanAdd))]
    [NotifyCanExecuteChangedFor(nameof(DelKeyCommand))]
    [NotifyCanExecuteChangedFor(nameof(AddKeyCommand))]
    [ObservableProperty]
    private string _keyValue = String.Empty;

    private bool CanAdd => !string.IsNullOrEmpty(KeyValue);

    [RelayCommand(CanExecute = nameof(CanAdd))]
    private void AddKey(string key)
    {
        if (Scenario.Keys.Contains(key))
        {
            return;
        }

        KeyValue = null;
        Scenario.Keys.Add(key);
        IsModified = true;
    }

    #endregion
}

public partial class PointItem : ObservableRecipient
{
    [ObservableProperty] private Point _location;

    [ObservableProperty] private string _title;
    [ObservableProperty] private ObservableCollection<ConnectorItem> input = new();
    [ObservableProperty] private ObservableCollection<ConnectorItem> output = new();
    [ObservableProperty] private s节点状态 status = s节点状态.未验证;

    public string? Plugin
    {
        get;
        set;
    }


    public string MerthodName
    {
        get;
        set;
    }
}

public enum s节点状态
{
    未验证,
    已验证,
    错误,
    初步验证
}

public partial class ConnectorItem : ObservableRecipient
{
    [ObservableProperty] private Point _anchor;

    [ObservableProperty] private object? _inputObject; //数据

    [ObservableProperty] private bool _isConnected;
    [ObservableProperty] private bool _isNotUsed = false;
    [ObservableProperty] private bool _isOut;
    [ObservableProperty] private bool _isSelf = false;

    private Type? _realType;

    public int AutoUnboxIndex
    {
        get;
        set;
    }

    public string TypeName
    {
        get;
        set;
    }

    public string Title
    {
        get;
        set;
    }

    /// <summary>
    /// 输出的类型
    /// </summary>
    [JsonConverter(typeof(TypeJsonConverter))]
    public Type Type
    {
        get;
        set;
    }

    /// <summary>
    /// 
    /// </summary>
    public Type RealType
    {
        get => _realType ?? Type;
        set => _realType = value;
    }

    public List<string>? Interfaces
    {
        get;
        set;
    }

    public PointItem Source
    {
        get;
        set;
    }

    partial void OnInputObjectChanged(object? value)
    {
        WeakReferenceMessenger.Default.Send(new CustomScenarioChangeMsg() { PointItem = Source, ConnectorItem = this });
    }
}

public class ConnectionItem
{
    public ConnectionItem(ConnectorItem source, ConnectorItem target)
    {
        Source = source;
        Target = target;

        Source.IsConnected = true;
        Target.IsConnected = true;
    }

    public ConnectorItem Source
    {
        get;
        set;
    }

    public ConnectorItem Target
    {
        get;
        set;
    }
}