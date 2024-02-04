#region

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Core.SDKs;
using Core.SDKs.CustomScenario;
using Core.SDKs.Services;
using Core.SDKs.Services.Plugin;
using Core.SDKs.Tools;
using Core.SDKs.Tools.Ex;
using log4net;
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

    [ObservableProperty] private ObservableCollection<ObservableCollection<object>> _nodeMethods = new();

    [ObservableProperty] private CustomScenario _scenario = new CustomScenario { IsActive = true };

    private Window _window;

    public TaskEditorViewModel()
    {
        PendingConnection = new PendingConnectionViewModel(this);
        GetAllMethods();
        var nodify2 = new PointItem
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
        var nodify3 = new PointItem
        {
            Title = "Tick",
            Location = new Point(0, 100)
        };
        nodify3.Output = new ObservableCollection<ConnectorItem>
        {
            new()
            {
                IsOut = true,
                Source = nodify3,
                Type = typeof(NodeConnectorClass),
                TypeName = BaseNodeMethodsGen.GetI18N(typeof(NodeConnectorClass).FullName),
                Title = "开始"
            }
        };
        Scenario.nodes.Add(nodify3);
        if (Scenario.UUID == null)
        {
            Scenario.UUID = Guid.NewGuid().ToString();
        }

        WeakReferenceMessenger.Default.Register<CustomScenarioChangeMsg>(this, (a, e) =>
        {
            IsModified = true;
            //Console.WriteLine(1);
            if (e.Type == 1)
            {
                if (e.Name == "Name")
                {
                    e.CustomScenario.nodes[0].Title = e.CustomScenario.Name;
                }

                return;
            }

            if (!Scenario.nodes.Contains(e.PointItem) && !NodeMethods.Any(a => a.Contains(e.PointItem)))
            {
                return;
            }

            if (e.ConnectorItem is not { InputObject: not null, SelfInputAble: false })
            {
                return;
            }

            if (e.PointItem.MerthodName == "一对N" && e.ConnectorItem.Title == "输出数量")
            {
                int? value = null;
                if (e.ConnectorItem.InputObject is int inputObject)
                {
                    value = inputObject;
                }
                else
                {
                    value = Convert.ToInt32((double)e.ConnectorItem.InputObject);
                }

                if (value > 10)
                {
                    value = 10;
                    e.ConnectorItem.InputObject = (double)10;
                }

                if (e.PointItem.Output.Count == value)
                {
                    return;
                }

                while (e.PointItem.Output.Count != value)
                {
                    if (e.PointItem.Output.Count > value)
                    {
                        var connectorItem = e.PointItem.Output[^1];
                        var connectionItems = Scenario.connections
                            .Where(connectionItem => connectionItem.Source == connectorItem).ToList();
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
                        e.PointItem.Output.Add(new ConnectorItem
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
        });

        //nodeMethods.Add("new PointItem(){Title = \"Test\"}");
    }

    public bool IsSaveInLocal => CustomScenarioManger.CustomScenarios.Contains(Scenario);

    public object ContentPresenter
    {
        get;
        set;
    }

    public PendingConnectionViewModel PendingConnection
    {
        get;
    }

    [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
    public static extern IntPtr GetForegroundWindow();

    [RelayCommand]
    private void VerifyNode()
    {
        Scenario.Run();
    }

    [RelayCommand]
    private void StopCustomScenario()
    {
        Scenario.Stop();
    }

    private void ToFirstVerify(bool notRealTime = false)
    {
        Scenario.Run(true);
    }

    [RelayCommand]
    private void SwitchConnector(ConnectorItem connector)
    {
        if (!connector.SelfInputAble)
        {
            return;
        }

        IsModified = true;

        connector.IsSelf = !connector.IsSelf;
        if (connector.IsSelf)
        {
            var connectionItems = Scenario.connections
                .Where(e => e.Source == connector || e.Target == connector).ToList();
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
        }

        // Scenario.nodes.ResetBindings();
    }

    [RelayCommand]
    private void AddNodes(PointItem pointItem)
    {
        IsModified = true;

        var item = new PointItem
        {
            Title = pointItem.Title,
            Plugin = pointItem.Plugin,
            ValueRef = pointItem.ValueRef,
            MerthodName = pointItem.MerthodName,
            Location = new Point(pointItem.Location.X, pointItem.Location.Y)
        };
        if (pointItem.Plugin != "Kitopia")
        {
            Scenario._plugs!.AddOrIncrease(pointItem.Plugin);
        }

        ObservableCollection<ConnectorItem> input = new();
        foreach (var connectorItem in pointItem.Input)
        {
            input.Add(new ConnectorItem
            {
                Anchor = new Point(connectorItem.Anchor.X, connectorItem.Anchor.Y),
                Source = item,
                TypeName = connectorItem.TypeName,
                Title = connectorItem.Title,
                Type = connectorItem.Type,
                RealType = connectorItem.RealType,
                InputObject = connectorItem.InputObject,
                AutoUnboxIndex = connectorItem.AutoUnboxIndex,
                IsSelf = connectorItem.IsSelf,
                SelfInputAble = connectorItem.SelfInputAble,
                IsOut = connectorItem.IsOut
            });
            var plugin = PluginManager.EnablePlugin.FirstOrDefault((e) => e.Value._dll == connectorItem.Type.Assembly)
                .Value;
            if (plugin is not null)
            {
                Scenario._plugs.AddOrIncrease(plugin.ToPlgString());
            }

            var plugin2 = PluginManager.EnablePlugin
                .FirstOrDefault((e) => e.Value._dll == connectorItem.RealType.Assembly).Value;
            if (plugin2 is not null)
            {
                Scenario._plugs.AddOrIncrease(plugin2.ToPlgString());
            }
        }

        ObservableCollection<ConnectorItem> output = new();
        foreach (var connectorItem in pointItem.Output)
        {
            var connectorItem1 = new ConnectorItem
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

            var plugin = PluginManager.EnablePlugin.FirstOrDefault((e) => e.Value._dll == connectorItem.Type.Assembly)
                .Value;
            if (plugin is not null)
            {
                Scenario._plugs.AddOrIncrease(plugin.ToPlgString());
            }

            var plugin2 = PluginManager.EnablePlugin
                .FirstOrDefault((e) => e.Value._dll == connectorItem.RealType.Assembly).Value;
            if (plugin2 is not null)
            {
                Scenario._plugs.AddOrIncrease(plugin2.ToPlgString());
            }

            output.Add(connectorItem1);
        }

        item.Input = input;
        item.Output = output;
        Scenario.nodes.Add(item);
    }

    [RelayCommand]
    private void CopyNode(PointItem pointItem)
    {
        IsModified = true;
        if (Scenario.nodes.IndexOf(pointItem) is 0 or 1)
        {
            return;
        }

        var item = new PointItem
        {
            Title = pointItem.Title,
            Plugin = pointItem.Plugin,
            ValueRef = pointItem.ValueRef,
            MerthodName = pointItem.MerthodName,
            Location = new Point(pointItem.Location.X + 40, pointItem.Location.Y + 40)
        };
        ObservableCollection<ConnectorItem> input = new();
        foreach (var connectorItem in pointItem.Input)
        {
            input.Add(new ConnectorItem
            {
                Anchor = new Point(connectorItem.Anchor.X, connectorItem.Anchor.Y),
                Source = item,
                TypeName = connectorItem.TypeName,
                Title = connectorItem.Title,
                Type = connectorItem.Type,
                RealType = connectorItem.RealType,
                InputObject = connectorItem.InputObject,
                AutoUnboxIndex = connectorItem.AutoUnboxIndex,
                IsSelf = connectorItem.IsSelf,
                SelfInputAble = connectorItem.SelfInputAble,
                IsOut = connectorItem.IsOut
            });
        }

        ObservableCollection<ConnectorItem> output = new();
        foreach (var connectorItem in pointItem.Output)
        {
            var connectorItem1 = new ConnectorItem
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
        var indexOf = Scenario.nodes.IndexOf(pointItem);
        if (indexOf is 0 or 1)
        {
            return;
        }

        var connectionItems = Scenario.connections
            .Where(e => e.Source.Source == pointItem || e.Target.Source == pointItem).ToList();
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

        Scenario._plugs.DelOrDecrease(pointItem.Plugin);
        foreach (var connectorItem in pointItem.Input)
        {
            var plugin = PluginManager.EnablePlugin.FirstOrDefault((e) => e.Value._dll == connectorItem.Type.Assembly)
                .Value;
            if (plugin is not null)
            {
                Scenario._plugs.DelOrDecrease(plugin.ToPlgString());
            }

            var plugin2 = PluginManager.EnablePlugin
                .FirstOrDefault((e) => e.Value._dll == connectorItem.RealType.Assembly).Value;
            if (plugin2 is not null)
            {
                Scenario._plugs.DelOrDecrease(plugin2.ToPlgString());
            }
        }

        foreach (var connectorItem in pointItem.Output)
        {
            var plugin = PluginManager.EnablePlugin.FirstOrDefault((e) => e.Value._dll == connectorItem.Type.Assembly)
                .Value;
            if (plugin is not null)
            {
                Scenario._plugs.DelOrDecrease(plugin.ToPlgString());
            }

            var plugin2 = PluginManager.EnablePlugin
                .FirstOrDefault((e) => e.Value._dll == connectorItem.RealType.Assembly).Value;
            if (plugin2 is not null)
            {
                Scenario._plugs.DelOrDecrease(plugin2.ToPlgString());
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
        OnPropertyChanged(nameof(IsSaveInLocal));
    }

    [RelayCommand(CanExecute = nameof(IsModified))]
    private void SaveAndQuitCustomScenario(Window window)
    {
        CleanUnusedNode();
        var dialog = new DialogContent()
        {
            Content = "是否确定保存并退出",
            Title = "保存并退出?",
            PrimaryButtonText = "确定",
            SecondaryButtonText = "取消",
            PrimaryAction = () =>
            {
                IsModified = false;
                CustomScenarioManger.Save(Scenario);
                OnPropertyChanged(nameof(IsSaveInLocal));
                Dispatcher.UIThread.InvokeAsync(() =>
                {
                    window.Close();
                });
            }
        };
        ((IContentDialog)ServiceManager.Services!.GetService(typeof(IContentDialog))!).ShowDialogAsync(ContentPresenter,
            dialog);
    }

    [RelayCommand]
    private void CleanUnusedNode()
    {
        for (var i = Scenario.nodes.Count - 1; i >= 2; i--)
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
                DelNode(Scenario.nodes[i]);
            }
        }
    }

    [RelayCommand]
    private void DisconnectConnector(ConnectorItem connector)
    {
        var connections = Scenario.connections.Where(e => e.Source == connector || e.Target == connector).ToList();
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
            var methods = new ObservableCollection<object>();
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
            Scenario.Stop();
            var dialog = new DialogContent()
            {
                Content = "是否确定不保存退出",
                Title = "不保存退出?",
                PrimaryButtonText = "保存并退出",
                SecondaryButtonText = "不保存",
                CloseButtonText = "取消",
                PrimaryAction = () =>
                {
                    IsModified = false;
                    CustomScenarioManger.Save(Scenario);
                    _window.Close();
                },
                SecondaryAction = () =>
                {
                    CustomScenarioManger.Reload(Scenario);
                    IsModified = false;
                    _window.Close();
                },
                CloseAction = () =>
                {
                    e.Cancel = true;
                }
            };
            ((IContentDialog)ServiceManager.Services!.GetService(typeof(IContentDialog))!)
                .ShowDialogAsync(null, dialog);
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

    #region 变量

    [NotifyPropertyChangedFor(nameof(valueCanAdd))]
    [NotifyCanExecuteChangedFor(nameof(AddValueCommand))]
    [ObservableProperty]
    private string? _valueValue = String.Empty;

    private bool valueCanAdd => !string.IsNullOrEmpty(ValueValue);

    [RelayCommand(CanExecute = nameof(valueCanAdd))]
    private void AddValue(string key)
    {
        if (Scenario.Values.ContainsKey(key))
        {
            return;
        }

        ValueValue = null;
        Scenario.Values.Add(key, new object());
        OnPropertyChanged(CommunityToolkit.Mvvm.ComponentModel.__Internals.__KnownINotifyPropertyChangedArgs.Values);
        IsModified = true;
    }

    [RelayCommand]
    private void DelValue(string key)
    {
        if (Scenario.Values.ContainsKey(key))
        {
            Scenario.Values.Remove(key);
        }

        IsModified = true;
    }

    #endregion
}