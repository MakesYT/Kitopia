#region

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Core.SDKs;
using Core.SDKs.CustomScenario;
using Core.SDKs.Services;
using Core.SDKs.Services.Config;
using Core.SDKs.Services.Plugin;
using log4net;
using Microsoft.Extensions.DependencyInjection;
using PluginCore;

#endregion

namespace Core.ViewModel.Pages.plugin;

public partial class PluginManagerPageViewModel : ObservableRecipient
{
    private static readonly ILog Log = LogManager.GetLogger(nameof(PluginManagerPageViewModel));
    private readonly TaskScheduler _scheduler = TaskScheduler.FromCurrentSynchronizationContext();
    [ObservableProperty] private ObservableCollection<PluginInfo> _items = new();

    public PluginManagerPageViewModel()
    {
        //Task.Factory.StartNew(LoadPluginsInfo, CancellationToken.None, TaskCreationOptions.None, _scheduler);
        Task.Run(LoadPluginsInfo);
    }

    private void LoadPluginsInfo()
    {
        Items.Clear();
        var pluginsDirectoryInfo = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory + "plugins");
        if (!pluginsDirectoryInfo.Exists)
        {
            Log.Debug($"插件目录不存在创建{pluginsDirectoryInfo.FullName}");
            pluginsDirectoryInfo.Create();
        }

        foreach (var directoryInfo in pluginsDirectoryInfo.EnumerateDirectories())
        {
            var last = directoryInfo.Name.Split("_").Last();
            if (File.Exists($"{directoryInfo.FullName}{Path.DirectorySeparatorChar}{last}.dll"))
            {
                var pluginInfoEx = PluginInfoTool.GetPluginInfoEx($"{directoryInfo.FullName}{Path.DirectorySeparatorChar}{last}.dll",
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
    private void RestartApp()
    {
        ServiceManager.Services.GetService<IApplicationService>()!.Restart();
    }

    [RelayCommand]
    private void Delete(PluginInfo pluginInfoEx)
    {
        var dialog = new DialogContent()
        {
            Title = $"删除{pluginInfoEx.PluginName}?",
            Content = "是否确定删除?\n他真的会丢失很久很久(不可恢复)",
            PrimaryButtonText = "确定",
            CloseButtonText = "取消",
            PrimaryAction = () =>
            {
                Switch(pluginInfoEx);
                if (!pluginInfoEx.UnloadFailed)
                {
                    var pluginsDirectoryInfo = new DirectoryInfo($"{AppDomain.CurrentDomain.BaseDirectory}plugins{Path.DirectorySeparatorChar}{pluginInfoEx.ToPlgString()}");
                    pluginsDirectoryInfo.Delete(true);
                    Task.Run(LoadPluginsInfo);
                }
            }
        };
        ((IContentDialog)ServiceManager.Services!.GetService(typeof(IContentDialog))!).ShowDialogAsync(null,
            dialog);
        
        
    }
    [RelayCommand]
    public void Switch(PluginInfo pluginInfoEx)
    {
        if (pluginInfoEx.IsEnabled)
        {
            //卸载插件

            Plugin.UnloadByPluginInfo(pluginInfoEx.ToPlgString(), out var weakReference);
            PluginManager.EnablePlugin.Remove(pluginInfoEx.ToPlgString());
            for (int i = 0; i < 10; i++)
            {
                GC.Collect(2, GCCollectionMode.Aggressive);
                GC.WaitForPendingFinalizers();
                Task.Delay(10).Wait();
            }
            

            ConfigManger.Config.EnabledPluginInfos.RemoveAll(e =>
                e.PluginId == pluginInfoEx.PluginId && e.Author == pluginInfoEx.Author &&
                e.VersionInt == pluginInfoEx.VersionInt);
            ConfigManger.Save();
            pluginInfoEx.IsEnabled = false;
            if (weakReference.IsAlive)
            {
                pluginInfoEx.UnloadFailed = true;
            }
            // Items.ResetBindings();
            CustomScenarioManger.LoadAll();
        }
        else
        {
            //加载插件
            //Plugin.NewPlugin(pluginInfoEx.Path, out var weakReference);
            PluginManager.EnablePlugin.Add(pluginInfoEx.ToPlgString(),
                new Plugin(pluginInfoEx.Path));
            ConfigManger.Config.EnabledPluginInfos.Add(pluginInfoEx);
            ConfigManger.Save();
            pluginInfoEx.IsEnabled = true;
            // Items.ResetBindings();
            CustomScenarioManger.ReCheck(true);
        }
    }

    [RelayCommand]
    public void ToPluginSettingPage(PluginInfo pluginInfoEx)
    {
        if (!pluginInfoEx.IsEnabled)
        {
            return;
        }

        ((INavigationPageService)ServiceManager.Services!.GetService(typeof(INavigationPageService))).Navigate(
            $"PluginSettingSelectPage_{pluginInfoEx.ToPlgString()}");
        
    }
}