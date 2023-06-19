﻿using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Core.SDKs.Services.Plugin;
using log4net;

namespace Core.ViewModel.Pages.plugin;

public partial class PluginManagerPageViewModel : ObservableRecipient
{
    private static readonly ILog Log = LogManager.GetLogger(nameof(PluginManagerPageViewModel));
    [ObservableProperty] private BindingList<PluginInfoEx> _items = new();
    private readonly TaskScheduler _scheduler = TaskScheduler.FromCurrentSynchronizationContext();

    public PluginManagerPageViewModel()
    {
        Task.Factory.StartNew(LoadPluginsInfo, CancellationToken.None, TaskCreationOptions.None, _scheduler);
    }

    private void LoadPluginsInfo()
    {
        DirectoryInfo pluginsDirectoryInfo = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory + "plugins");
        if (!pluginsDirectoryInfo.Exists)
        {
            Log.Debug($"插件目录不存在创建{pluginsDirectoryInfo.FullName}");
            pluginsDirectoryInfo.Create();
        }
#if DEBUG
        Log.Debug("Debug加载测试插件");
        File.Copy(@"D:\WPF.net\uToolkitopia\PluginDemo\bin\Debug\net7.0-windows\PluginDemo.dll",
            pluginsDirectoryInfo.FullName + "\\PluginDemo.dll", true);

#endif

        foreach (FileInfo enumerateFile in pluginsDirectoryInfo.EnumerateFiles())
        {
            if (enumerateFile.Extension.Equals(".dll"))
            {
                Log.Debug($"加载插件:{enumerateFile.FullName}");
                var pluginInfoEx = Plugin.GetPluginInfoEx(enumerateFile.FullName, out var alcWeakRef);
                if (pluginInfoEx.Author != "error")
                {
                    Items.Add(pluginInfoEx);
                }

                for (int i = 0; alcWeakRef.IsAlive && (i < 10); i++)
                {
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                }
            }
        }
    }

    [RelayCommand]
    public async Task Switch(PluginInfoEx pluginInfoEx)
    {
        if (pluginInfoEx.IsEnabled)
        {
            //卸载插件
            if (PluginManager.EnablePlugin.TryGetValue($"{pluginInfoEx.Author}_{pluginInfoEx.PluginId}",
                    out var weakReference))
            {
                PluginManager.EnablePlugin.Remove($"{pluginInfoEx.Author}_{pluginInfoEx.PluginId}");
                pluginInfoEx.IsEnabled = false;
                if (weakReference.TryGetTarget(out var plugin))
                {
                    plugin.Unload(out var weakReferenceP);
                    for (int i = 0; weakReferenceP.IsAlive && (i < 10); i++)
                    {
                        GC.Collect(2, GCCollectionMode.Aggressive, true);
                        GC.WaitForPendingFinalizers();
                    }
                }
            }
        }
        else
        {
            //加载插件
            Plugin.NewPlugin(pluginInfoEx.Path, out var weakReference);
            PluginManager.EnablePlugin.Add($"{pluginInfoEx.Author}_{pluginInfoEx.PluginId}", weakReference);
            pluginInfoEx.IsEnabled = true;
        }
    }
}