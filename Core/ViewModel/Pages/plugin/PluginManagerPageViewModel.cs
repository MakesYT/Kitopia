using System.ComponentModel;
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
        if (!Directory.Exists(pluginsDirectoryInfo.FullName + "\\PluginDemo"))
        {
            Directory.CreateDirectory(pluginsDirectoryInfo.FullName + "\\PluginDemo");
        }

        File.Copy(@"D:\WPF.net\uToolkitopia\PluginDemo\bin\Debug\net7.0-windows\PluginDemo.dll",
            pluginsDirectoryInfo.FullName + "\\PluginDemo\\PluginDemo.dll", true);

#endif
        foreach (DirectoryInfo directoryInfo in pluginsDirectoryInfo.EnumerateDirectories())
        {
            if (File.Exists($"{directoryInfo.FullName}\\{directoryInfo.Name}.dll"))
            {
                Log.Debug($"加载插件:{directoryInfo.Name}.dll");
                var pluginInfoEx = Plugin.GetPluginInfoEx($"{directoryInfo.FullName}\\{directoryInfo.Name}.dll",
                    out var alcWeakRef);
                if (pluginInfoEx.Author != "error")
                {
                    Items.Add(pluginInfoEx);
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
            while (weakReference.IsAlive)
            {
                GC.Collect(2, GCCollectionMode.Aggressive, true, true);
                GC.WaitForPendingFinalizers();
            }
        }
        else
        {
            //加载插件
            //Plugin.NewPlugin(pluginInfoEx.Path, out var weakReference);
            PluginManager.EnablePlugin.Add($"{pluginInfoEx.Author}_{pluginInfoEx.PluginId}",
                new Plugin(pluginInfoEx.Path));
            pluginInfoEx.IsEnabled = true;
        }
    }
}