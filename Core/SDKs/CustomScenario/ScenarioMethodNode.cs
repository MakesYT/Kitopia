using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using Core.SDKs.Services.Config;

namespace Core.SDKs.CustomScenario;

public enum s节点状态
{
    未验证,
    已验证,
    错误,
    初步验证
}

public partial class ScenarioMethodNode : ObservableRecipient
{
    [property: JsonConverter(typeof(PointJsonConverter))]
    [JsonConverter(typeof(PointJsonConverter))]
    [ObservableProperty]
    private Point _location;

    [ObservableProperty] private string _title;
    [ObservableProperty] private ObservableCollection<ConnectorItem> input = new();
    [ObservableProperty] private ObservableCollection<ConnectorItem> output = new();
    [ObservableProperty] private s节点状态 status = s节点状态.未验证;

    public ScenarioMethodInfo ScenarioMethodInfo { get; set; }

    public string? ValueRef { get; set; }
}