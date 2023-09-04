using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Core.ViewModel.TaskEditor;

namespace Core.SDKs.Services.Config;

public partial class CustomScenario : ObservableRecipient
{
    /// <summary>
    /// 手动执行
    /// </summary>
    [ObservableProperty] [NotifyPropertyChangedRecipients]
    public bool executionManual = true;


    [ObservableProperty] [NotifyPropertyChangedRecipients]
    public List<string> keys;

    /// <summary>
    /// 间隔指定时间执行
    /// </summary>
    [ObservableProperty] [NotifyPropertyChangedRecipients]
    public bool executionIntervalSpecifies = false;

    [ObservableProperty] [NotifyPropertyChangedRecipients]
    public List<TimeSpan> intervalSpecifiesTimeSpan;

    /// <summary>
    /// 指定时间执行
    /// </summary>
    [ObservableProperty] [NotifyPropertyChangedRecipients]
    public bool executionScheduleTime = false;

    [ObservableProperty] [NotifyPropertyChangedRecipients]
    public TimeSpan scheduleTime;

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