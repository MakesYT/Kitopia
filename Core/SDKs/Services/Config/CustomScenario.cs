using Core.ViewModel.TaskEditor;

namespace Core.SDKs.Services.Config;

public class CustomScenario
{
    public string? UUID
    {
        get;
        set;
    }

    public List<PointItem> nodes = new();
    public List<ConnectionItem> connections = new();

    public string Name
    {
        get;
        set;
    } = "";

    public string Description
    {
        get;
        set;
    } = "";
}