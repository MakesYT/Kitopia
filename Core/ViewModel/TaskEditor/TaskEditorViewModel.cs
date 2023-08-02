using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
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

    [ObservableProperty] private ObservableCollection<object> _nodeMethods = new();
    [ObservableProperty] private ObservableCollection<object> nodes = new();

    [ObservableProperty] private ObservableCollection<ConnectionItem> connections = new();

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
        //nodeMethods.Add("new PointItem(){Title = \"Test\"}");
        NodeMethods.Add(new CheckBox() { Content = "2" });
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