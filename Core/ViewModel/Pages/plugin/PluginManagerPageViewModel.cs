using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Core.SDKs.Services;
using Core.SDKs.Services.Config;
using Core.SDKs.Services.Plugin;
using log4net;

namespace Core.ViewModel.Pages.plugin;

public partial class PluginManagerPageViewModel : ObservableRecipient
{
    private static readonly ILog Log = LogManager.GetLogger(nameof(PluginManagerPageViewModel));
    [ObservableProperty] private BindingList<PluginInfoEx> _items = new();

    public PluginManagerPageViewModel()
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
                var pluginInfo = Plugin.GetPluginInfo(enumerateFile.FullName);
                if (ConfigManger.Config.EnabledPluginInfos.Contains(pluginInfo))
                {
                    var pluginInfoEx = new PluginInfoEx()
                    {
                        Author = pluginInfo.Author,
                        Error = "",
                        IsEnabled = true,
                        PluginId = pluginInfo.PluginId,
                        PluginName = pluginInfo.PluginName,
                        Version = pluginInfo.Version,
                        VersionInt = pluginInfo.VersionInt
                    };
                    Items.Add(pluginInfoEx);
                }
                else if (ConfigManger.Config.EnabledPluginInfos.Exists(e =>
                         {
                             if (e.PluginName != pluginInfo.PluginName)
                             {
                                 return false;
                             }

                             if (e.Author != pluginInfo.Author)
                             {
                                 return false;
                             }

                             if (e.VersionInt != pluginInfo.VersionInt)
                             {
                                 return true;
                             }

                             return false;
                         })) //有这个插件但是版本不对
                {
                    var pluginInfoEx = new PluginInfoEx()
                    {
                        Author = pluginInfo.Author,
                        Error = "插件版本不一致",
                        IsEnabled = false,
                        PluginId = pluginInfo.PluginId,
                        PluginName = pluginInfo.PluginName,
                        Version = pluginInfo.Version,
                        VersionInt = pluginInfo.VersionInt
                    };
                    Items.Add(pluginInfoEx);
                }
                else
                {
                    var pluginInfoEx = new PluginInfoEx()
                    {
                        Author = pluginInfo.Author,
                        Error = "",
                        IsEnabled = false,
                        PluginId = pluginInfo.PluginId,
                        PluginName = pluginInfo.PluginName,
                        Version = pluginInfo.Version,
                        VersionInt = pluginInfo.VersionInt
                    };
                    Items.Add(pluginInfoEx);
                }
            }
        }
    }

    [RelayCommand]
    public void Click()
    {
        ((INavigationPageService)ServiceManager.Services!.GetService(typeof(INavigationPageService))).Navigate("设置");
    }
}