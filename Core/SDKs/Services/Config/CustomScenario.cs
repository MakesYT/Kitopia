using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Core.ViewModel.TaskEditor;

namespace Core.SDKs.Services.Config;

public partial class CustomScenario : ObservableRecipient
{
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

    [ObservableProperty] [NotifyPropertyChangedRecipients]
    public string _name = "任务";

    [ObservableProperty] [NotifyPropertyChangedRecipients]
    public string _description = "";
}