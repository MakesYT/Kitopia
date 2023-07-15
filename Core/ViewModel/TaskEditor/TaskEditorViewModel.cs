using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

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

    [ObservableProperty] private ObservableCollection<PointItem> nodes = new();

    [ObservableProperty] public ObservableCollection<ConnectionItem> connections = new();

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
        var welcome = new PointItem
        {
            Title = "Welcome",
        };
        welcome.Input = new ObservableCollection<ConnectorItem>
        {
            new ConnectorItem
            {
                Source = welcome,
                Title = "In"
            }
        };
        welcome.Output = new ObservableCollection<ConnectorItem>
        {
            new ConnectorItem
            {
                IsOut = true,
                Source = welcome,
                Title = "Out"
            }
        };

        var nodify = new PointItem
        {
            Title = "To Nodify",
        };
        nodify.Input = new ObservableCollection<ConnectorItem>
        {
            new ConnectorItem
            {
                Source = nodify,
                Title = "In"
            }
        };
        Nodes.Add(welcome);
        Nodes.Add(nodify);
    }

    public void Connect(ConnectorItem source, ConnectorItem target)
    {
        Connections.Add(new ConnectionItem(source, target));
    }
}

public class PointItem
{
    public string Title
    {
        get;
        set;
    }

    public ObservableCollection<ConnectorItem> Input
    {
        get;
        set;
    } = new ObservableCollection<ConnectorItem>();

    public ObservableCollection<ConnectorItem> Output
    {
        get;
        set;
    } = new ObservableCollection<ConnectorItem>();
}

public partial class ConnectorItem : ObservableRecipient
{
    [ObservableProperty] private Point _anchor;

    [ObservableProperty] private bool _isConnected;

    [ObservableProperty] private bool _isOut;

    public string Title
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