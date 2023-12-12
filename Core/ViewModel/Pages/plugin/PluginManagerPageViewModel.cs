#region

using System.ComponentModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Core.SDKs.CustomScenario;
using Core.SDKs.Services;
using Core.SDKs.Services.Config;
using Core.SDKs.Services.Plugin;
using log4net;

#endregion

namespace Core.ViewModel.Pages.plugin;

public partial class PluginManagerPageViewModel : ObservableRecipient
{
    private static readonly ILog Log = LogManager.GetLogger(nameof(PluginManagerPageViewModel));
    private readonly TaskScheduler _scheduler = TaskScheduler.FromCurrentSynchronizationContext();
    [ObservableProperty] private BindingList<PluginInfoEx> _items = new();

    public PluginManagerPageViewModel()
    {
        //Task.Factory.StartNew(LoadPluginsInfo, CancellationToken.None, TaskCreationOptions.None, _scheduler);
        Task.Run(LoadPluginsInfo);
    }

    private void LoadPluginsInfo()
    {
        var pluginsDirectoryInfo = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory + "plugins");
        if (!pluginsDirectoryInfo.Exists)
        {
            Log.Debug($"插件目录不存在创建{pluginsDirectoryInfo.FullName}");
            pluginsDirectoryInfo.Create();
        }

        foreach (var directoryInfo in pluginsDirectoryInfo.EnumerateDirectories())
        {
            if (File.Exists($"{directoryInfo.FullName}\\{directoryInfo.Name}.dll"))
            {
                Log.Debug($"加载插件Info:{directoryInfo.Name}.dll");
                var pluginInfoEx = PluginExTool.GetPluginInfoEx($"{directoryInfo.FullName}\\{directoryInfo.Name}.dll",
                    out var alcWeakRef);
                if (pluginInfoEx.Version != "error")
                {
                    if (PluginManager.EnablePlugin.ContainsKey(pluginInfoEx.ToPlgString()))
                    {
                        pluginInfoEx.IsEnabled = true;
                    }

                    Task.Factory.StartNew(() =>
                    {
                        Items.Add(pluginInfoEx);
                    }, CancellationToken.None, TaskCreationOptions.None, _scheduler);
                }

                while (alcWeakRef.IsAlive)
                {
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                }
            }
        }
    }

    [RelayCommand]
    public void Switch(PluginInfoEx pluginInfoEx)
    {
        if (pluginInfoEx.IsEnabled)
        {
            //卸载插件


            Plugin.UnloadByPluginInfo(pluginInfoEx, out var weakReference);
            PluginManager.EnablePlugin.Remove(pluginInfoEx.ToPlgString());
            while (weakReference.IsAlive)
            {
                GC.Collect(2, GCCollectionMode.Aggressive);
                GC.WaitForPendingFinalizers();
            }

            ConfigManger.Config.EnabledPluginInfos.Remove(pluginInfoEx.ToPluginInfo());
            ConfigManger.Save();
            pluginInfoEx.IsEnabled = false;
            Items.ResetBindings();
            CustomScenarioManger.LoadAll();
        }
        else
        {
            //加载插件
            //Plugin.NewPlugin(pluginInfoEx.Path, out var weakReference);
            PluginManager.EnablePlugin.Add(pluginInfoEx.ToPlgString(),
                new Plugin(pluginInfoEx.Path));
            ConfigManger.Config.EnabledPluginInfos.Add(pluginInfoEx.ToPluginInfo());
            ConfigManger.Save();
            pluginInfoEx.IsEnabled = true;
            Items.ResetBindings();
            CustomScenarioManger.ReCheck(true);
        }
    }

    [RelayCommand]
    public void ToPluginSettingPage(PluginInfoEx pluginInfoEx)
    {
        if (!pluginInfoEx.IsEnabled)
        {
            return;
        }

        ((INavigationPageService)ServiceManager.Services!.GetService(typeof(INavigationPageService))).Navigate(
            "PluginSetting");
        ((PluginSettingViewModel)ServiceManager.Services!.GetService(typeof(PluginSettingViewModel)))
            .ChangePlugin(pluginInfoEx);
    }
}