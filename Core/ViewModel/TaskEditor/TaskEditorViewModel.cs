using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Core.SDKs.Services.Plugin;
using PluginCore.Attribute;

namespace Core.ViewModel.TaskEditor;

public partial class TaskEditorViewModel : ObservableRecipient
{
    public ICommand DisconnectConnectorCommand
    {
        get;
    }

    public PendingConnectionViewModel PendingConnection
    {
        get;
    }

    [ObservableProperty] private BindingList<object> _nodeMethods = new();


    [RelayCommand]
    private void AddNodes(PointItem pointItem)
    {
        var item = new PointItem()
        {
            Title = pointItem.Title,
            Plugin = pointItem.Plugin,
            Location = new Point(pointItem.Location.X, pointItem.Location.Y)
        };
        ObservableCollection<ConnectorItem> input = new();
        foreach (var connectorItem in pointItem.Input)
        {
            input.Add(new ConnectorItem()
            {
                Anchor = new Point(connectorItem.Anchor.X, connectorItem.Anchor.Y),
                Source = item,
                Title = connectorItem.Title,
                Type = connectorItem.Type,
                InputObject = connectorItem.InputObject,
                IsConnected = connectorItem.IsConnected,
                IsSelf = connectorItem.IsSelf,
                IsOut = connectorItem.IsOut
            });
        }

        ObservableCollection<ConnectorItem> output = new();
        foreach (var connectorItem in pointItem.Output)
        {
            output.Add(new ConnectorItem()
            {
                Anchor = new Point(connectorItem.Anchor.X, connectorItem.Anchor.Y),
                Source = item,
                Title = connectorItem.Title,
                Type = connectorItem.Type,
                IsConnected = connectorItem.IsConnected,
                IsOut = connectorItem.IsOut
            });
        }

        item.Input = input;
        item.Output = output;
        Nodes.Add(item);
        //OnPropertyChanged(nameof(Nodes));
    }

    [ObservableProperty] private ObservableCollection<PointItem> nodes = new();

    [ObservableProperty] private ObservableCollection<ConnectionItem> connections = new();

    private void GetAllMethods()
    {
        PointItem String = new PointItem()
        {
            Plugin = "Kitopia",
            Title = "字符串"
        };
        ObservableCollection<ConnectorItem> StringoutItems = new()
        {
            new ConnectorItem()
            {
                Source = String,
                Type = typeof(string),
                Title = "字符串",
                IsOut = true
            }
        };
        String.Output = StringoutItems;
        ObservableCollection<ConnectorItem> StringinItems = new()
        {
            new ConnectorItem()
            {
                Source = String,
                Type = typeof(string),
                Title = "字符串",
                IsSelf = true
            }
        };
        String.Input = StringinItems;
        NodeMethods.Add(String);


        foreach (var (key, value) in PluginManager.EnablePlugin)
        {
            foreach (var methodInfo in value.GetMethodInfos())
            {
                if (methodInfo.GetCustomAttribute(typeof(PluginMethod)) is not null)
                {
                    var customAttribute = (PluginMethod)(methodInfo.GetCustomAttribute(typeof(PluginMethod)));
                    PointItem pointItem = new PointItem()
                    {
                        Plugin = key,
                        Title = $"{key}_{customAttribute.Name}"
                    };
                    ObservableCollection<ConnectorItem> inpItems = new();
                    for (var index = 0; index < methodInfo.GetParameters().Length; index++)
                    {
                        var parameterInfo = methodInfo.GetParameters()[index];
                        inpItems.Add(new ConnectorItem()
                        {
                            Source = pointItem,
                            Type = parameterInfo.ParameterType,
                            Title = parameterInfo.ParameterType.Name,
                        });
                        //Log.Debug($"参数{index}:类型为{parameterInfo.ParameterType}");
                    }

                    if (methodInfo.ReturnParameter.ParameterType != typeof(void))
                    {
                        ObservableCollection<ConnectorItem> outItems = new()
                        {
                            new ConnectorItem()
                            {
                                Source = pointItem,
                                Type = methodInfo.ReturnParameter.ParameterType,
                                Title = methodInfo.ReturnParameter.ParameterType.Name,
                                IsOut = true
                            }
                        };
                        pointItem.Output = outItems;
                    }


                    pointItem.Input = inpItems;
                    NodeMethods.Add(pointItem);
                }

                //Log.Debug($"输出:类型为{methodInfo.ReturnParameter.ParameterType}");
            }
        }
    }

    public TaskEditorViewModel()
    {
        PendingConnection = new PendingConnectionViewModel(this);
        DisconnectConnectorCommand = new RelayCommand<ConnectorItem>(connector =>
        {
            var connection =
                Enumerable.First<ConnectionItem>(Connections, x => x.Source == connector || x.Target == connector);
            connection.Source.IsConnected =
                false; // This is not correct if there are multiple connections to the same connector
            connection.Target.IsConnected = false;
            Connections.Remove(connection);
        });
        GetAllMethods();
        var nodify2 = new PointItem()
        {
            Title = "任务1",
        };
        nodify2.Output = new ObservableCollection<ConnectorItem>
        {
            new ConnectorItem
            {
                IsOut = true,
                Source = nodify2,
                Title = "开始"
            }
        };
        Nodes.Add(nodify2);
        //nodeMethods.Add("new PointItem(){Title = \"Test\"}");
    }

    public void Connect(ConnectorItem source, ConnectorItem target)
    {
        if (source.IsConnected)
        {
            var connectionsToRemove = Connections
                .Where(e => e.Source == source)
                .ToList();

            foreach (var connection in connectionsToRemove)
            {
                connection.Source.IsConnected = false;
                connection.Target.IsConnected = false;
                Connections.Remove(connection);
            }
        }

        if (target.IsConnected)
        {
            var connectionsToRemove = Connections
                .Where(e => e.Target == target)
                .ToList();

            foreach (var connection in connectionsToRemove)
            {
                connection.Source.IsConnected = false;
                connection.Target.IsConnected = false;
                Connections.Remove(connection);
            }
        }

        OnPropertyChanged(nameof(Connections));
        Connections.Add(new ConnectionItem(source, target));
    }
}

public partial class PointItem : ObservableRecipient
{
    public string Plugin
    {
        get;
        set;
    }

    [ObservableProperty] private Point _location;

    [ObservableProperty] private string _title;

    [ObservableProperty] private ObservableCollection<ConnectorItem> input = new();
    [ObservableProperty] private ObservableCollection<ConnectorItem> output = new();
}

public partial class ConnectorItem : ObservableRecipient
{
    [ObservableProperty] private Point _anchor;

    [ObservableProperty] private bool _isConnected;

    [ObservableProperty] private bool _isOut;
    [ObservableProperty] private bool _isSelf = false;
    [ObservableProperty] private object? _inputObject;

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