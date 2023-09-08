#region

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Core.SDKs.CustomScenario;
using Core.SDKs.Services;
using Core.SDKs.Services.Config;
using Core.SDKs.Services.Plugin;
using Core.SDKs.Tools;
using PluginCore;
using PluginCore.Attribute;

#endregion

namespace Core.ViewModel.TaskEditor;

public partial class TaskEditorViewModel : ObservableRecipient
{
    public object ContentPresenter
    {
        get;
        set;
    }

    public PendingConnectionViewModel PendingConnection
    {
        get;
    }

    [ObservableProperty] private BindingList<BindingList<object>> _nodeMethods = new();


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
        //OnPropertyChanged(nameof(Nodes));
    }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCustomScenarioCommand))]
    [NotifyCanExecuteChangedFor(nameof(SaveAndQuitCustomScenarioCommand))]
    public bool _isModified = false;

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
                return;
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

    [ObservableProperty] private CustomScenario _scenario = new CustomScenario() { IsActive = true };

    private void GetAllMethods()
    {
        BaseNodeMethodsGen.GenBaseNodeMethods(NodeMethods);


        foreach (var (key, value) in PluginManager.EnablePlugin)
        {
            var methods = new BindingList<object>();
            foreach (var (tm, methodInfo) in value.GetMethodInfos())
            {
                if (methodInfo.GetCustomAttribute(typeof(PluginMethod)) is not null)
                {
                    var customAttribute = (PluginMethod)methodInfo.GetCustomAttribute(typeof(PluginMethod));
                    var pointItem = new PointItem()
                    {
                        Plugin = key,
                        MerthodName = tm,
                        Title = $"{key}_{customAttribute.Name}"
                    };
                    ObservableCollection<ConnectorItem> inpItems = new();
                    inpItems.Add(new ConnectorItem()
                    {
                        Source = pointItem,
                        Type = typeof(NodeConnectorClass),
                        Title = "流输入",
                        TypeName = "节点"
                    });
                    int autoUnboxIndex = 0;
                    for (var index = 0; index < methodInfo.GetParameters().Length; index++)
                    {
                        var parameterInfo = methodInfo.GetParameters()[index];
                        if (parameterInfo.ParameterType.GetCustomAttribute(typeof(AutoUnbox)) is not null)
                        {
                            autoUnboxIndex++;
                            var type = parameterInfo.ParameterType;
                            foreach (var memberInfo in type.GetProperties())
                            {
                                List<string>? interfaces = null;
                                if (!memberInfo.PropertyType.FullName.StartsWith("System."))
                                {
                                    interfaces = new();
                                    foreach (var @interface in memberInfo.PropertyType.GetInterfaces())
                                    {
                                        interfaces.Add(@interface.FullName);
                                    }
                                }


                                inpItems.Add(new ConnectorItem()
                                {
                                    Source = pointItem,
                                    Type = memberInfo.PropertyType,
                                    AutoUnboxIndex = autoUnboxIndex,
                                    Interfaces = interfaces,
                                    Title = customAttribute.GetParameterName(memberInfo.Name),
                                    TypeName = BaseNodeMethodsGen.GetI18N(memberInfo.PropertyType.FullName),
                                });
                            }
                        }
                        else
                        {
                            inpItems.Add(new ConnectorItem()
                            {
                                Source = pointItem,
                                Type = parameterInfo.ParameterType,
                                Title = customAttribute.GetParameterName(parameterInfo.Name),
                                TypeName = BaseNodeMethodsGen.GetI18N(parameterInfo.ParameterType.FullName)
                            });
                        }

                        //Log.Debug($"参数{index}:类型为{parameterInfo.ParameterType}");
                    }

                    if (methodInfo.ReturnParameter.ParameterType != typeof(void))
                    {
                        ObservableCollection<ConnectorItem> outItems = new();
                        if (methodInfo.ReturnParameter.ParameterType.GetCustomAttribute(typeof(AutoUnbox)) is not null)
                        {
                            autoUnboxIndex++;
                            var type = methodInfo.ReturnParameter.ParameterType;
                            foreach (var memberInfo in type.GetProperties())
                            {
                                List<string>? interfaces = null;
                                if (!memberInfo.PropertyType.FullName.StartsWith("System."))
                                {
                                    interfaces = new();
                                    foreach (var @interface in memberInfo.PropertyType.GetInterfaces())
                                    {
                                        interfaces.Add(@interface.FullName);
                                    }
                                }

                                outItems.Add(new ConnectorItem()
                                {
                                    Source = pointItem,
                                    Type = memberInfo.PropertyType,
                                    AutoUnboxIndex = autoUnboxIndex,
                                    Interfaces = interfaces,
                                    Title = customAttribute.GetParameterName(memberInfo.Name),
                                    TypeName = BaseNodeMethodsGen.GetI18N(memberInfo.PropertyType.FullName),
                                    IsOut = true
                                });
                            }
                        }
                        else
                        {
                            List<string> interfaces = new();
                            foreach (var @interface in methodInfo.ReturnParameter.ParameterType.GetInterfaces())
                            {
                                interfaces.Add(@interface.FullName);
                            }


                            outItems.Add(new ConnectorItem()
                            {
                                Source = pointItem,
                                Type = methodInfo.ReturnParameter.ParameterType,
                                Title = customAttribute.GetParameterName("return"),
                                Interfaces = interfaces,
                                TypeName =
                                    BaseNodeMethodsGen.GetI18N(methodInfo.ReturnParameter.ParameterType.FullName),
                                IsOut = true
                            });
                        }


                        pointItem.Output = outItems;
                    }


                    pointItem.Input = inpItems;

                    methods.Add(pointItem);
                }

                //Log.Debug($"输出:类型为{methodInfo.ReturnParameter.ParameterType}");
            }

            NodeMethods.Add(methods);
        }
    }

    public TaskEditorViewModel()
    {
        Scenario.PropertyChanged += (e, s) =>
        {
            if (s.PropertyName == "Name")
            {
                Scenario.nodes[0].Title = Scenario.Name;
            }

            IsModified = true;
        };
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

    public void Load(CustomScenario customScenario)
    {
        Scenario = customScenario;
        Scenario.PropertyChanged += (e, s) =>
        {
            switch (s.PropertyName)
            {
                case "Name":
                {
                    Scenario.nodes[0].Title = Scenario.Name;
                    break;
                }
            }

            IsModified = true;
        };
    }


    private Window _window;

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
            ((IToastService)ServiceManager.Services!.GetService(typeof(IToastService))!).showMessageBoxW(
                "不保存退出?", "是否确定不保存退出", "取消", "不保存", "保存并退出",
                () =>
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
                }
            );
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
    public string Plugin
    {
        get;
        set;
    }

    public string MerthodName
    {
        get;
        set;
    }

    [ObservableProperty] private Point _location;

    [ObservableProperty] private string _title;
    [ObservableProperty] private s节点状态 status = s节点状态.未验证;
    [ObservableProperty] private ObservableCollection<ConnectorItem> input = new();
    [ObservableProperty] private ObservableCollection<ConnectorItem> output = new();
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

    [ObservableProperty] private bool _isConnected;
    [ObservableProperty] private bool _isNotUsed = false;
    [ObservableProperty] private bool _isOut;
    [ObservableProperty] private bool _isSelf = false;
    [ObservableProperty] private object? _inputObject; //数据

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

    public Type Type
    {
        get;
        set;
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