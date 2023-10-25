using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Core.SDKs.CustomScenario;

public enum s节点状态
{
    未验证,
    已验证,
    错误,
    初步验证
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