namespace Core.SDKs.CustomScenario;

public class ConnectionItem
{
    public ConnectionItem(ConnectorItem source, ConnectorItem target)
    {
        Source = source;
        Target = target;

        Source.IsConnected = true;
        Target.IsConnected = true;
    }

    public ConnectionItem()
    {
    }

    public ConnectorItem Source { get; set; }

    public ConnectorItem Target { get; set; }
}