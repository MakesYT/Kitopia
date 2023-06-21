using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Core.SDKs.Services.Plugin;
using log4net;

namespace Core.ViewModel.Pages.plugin;

public partial class PluginSettingViewModel : ObservableRecipient
{
    private static readonly ILog Log = LogManager.GetLogger(nameof(PluginSettingViewModel));
    [ObservableProperty] private BindingList<PluginSettingItem> _settingItems = new();

    public void ChangePlugin(PluginInfoEx pluginInfoEx)
    {
    }
}