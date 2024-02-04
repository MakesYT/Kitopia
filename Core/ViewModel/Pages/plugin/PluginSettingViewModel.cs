#region

using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Core.SDKs.Services.Plugin;
using log4net;
using PluginCore;

#endregion

namespace Core.ViewModel.Pages.plugin;

public partial class PluginSettingViewModel : ObservableRecipient
{
    private static readonly ILog Log = LogManager.GetLogger(nameof(PluginSettingViewModel));
    [ObservableProperty] private ObservableCollection<PluginSettingItem> _settingItems = new();

    public void ChangePlugin(PluginInfo pluginInfoEx) =>
        // var plugin = PluginManager.EnablePlugin[$"{pluginInfoEx.Author}_{pluginInfoEx.PluginId}"];
        SettingItems.Add(new PluginSettingItem()
        {
        });
}