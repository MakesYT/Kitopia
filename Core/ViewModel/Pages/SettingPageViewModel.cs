#region

using System.Collections.ObjectModel;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Core.SDKs.HotKey;
using Core.SDKs.Services;
using Core.SDKs.Services.Config;
using log4net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using PluginCore;

#endregion

namespace Core.ViewModel.Pages;

public partial class SettingPageViewModel : ObservableRecipient
{
    private static readonly ILog log = LogManager.GetLogger("SettingPageViewModel");

    [ObservableProperty] private bool _截图直接复制到剪贴板;

    [ObservableProperty] private bool _autoStart;
    [ObservableProperty] private bool _autoStartEverything;

    [ObservableProperty] private bool _canReadClipboard;
    [ObservableProperty] private ObservableCollection<string> _ignoreItems;
    [ObservableProperty] private int _inputSmoothingMilliseconds;

    private bool _isInitializin = true;
    [ObservableProperty] private int _maxHistory;

    [ObservableProperty] private ObservableCollection<int> _maxHistoryOptions = new() { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
    [ObservableProperty] private MouseHookType _mouseKey;

    [ObservableProperty] private int _mouseKeyInverval;

    [ObservableProperty] private ObservableCollection<MouseHookType> _mouseKeyOptions = new()
        { MouseHookType.鼠标左键, MouseHookType.鼠标右键, MouseHookType.鼠标中键, MouseHookType.鼠标侧键1, MouseHookType.鼠标侧键2 };

    [ObservableProperty] private string _themeChoice;

    [ObservableProperty] private ObservableCollection<string> _themeChoiceOptions = new() { "跟随系统", "深色", "浅色" };
    [ObservableProperty] private bool _useEverything;

    public SettingPageViewModel()
    {
        WeakReferenceMessenger.Default.Register<string, string>(this, "ConfigSave", (s, o) =>
        {
            ThemeChoice = ConfigManger.Config.themeChoice;
            AutoStart = ConfigManger.Config.autoStart;
            AutoStartEverything = ConfigManger.Config.autoStartEverything;
            UseEverything = ConfigManger.Config.useEverything;
            MaxHistory = ConfigManger.Config.maxHistory;
            CanReadClipboard = ConfigManger.Config.canReadClipboard;
            IgnoreItems = new ObservableCollection<string>(ConfigManger.Config.ignoreItems);
            InputSmoothingMilliseconds = ConfigManger.Config.inputSmoothingMilliseconds;
            MouseKey = ConfigManger.Config.mouseKey;
            MouseKeyInverval = ConfigManger.Config.mouseKeyInverval;
            截图直接复制到剪贴板 = ConfigManger.Config.截图直接复制到剪贴板;
        });
        _themeChoice = ConfigManger.Config.themeChoice;
        _autoStart = ConfigManger.Config.autoStart;
        _autoStartEverything = ConfigManger.Config.autoStartEverything;
        _useEverything = ConfigManger.Config.useEverything;
        _maxHistory = ConfigManger.Config.maxHistory;
        _canReadClipboard = ConfigManger.Config.canReadClipboard;
        _ignoreItems = new ObservableCollection<string>(ConfigManger.Config.ignoreItems);
        _inputSmoothingMilliseconds = ConfigManger.Config.inputSmoothingMilliseconds;
        _mouseKey = ConfigManger.Config.mouseKey;
        _mouseKeyInverval = ConfigManger.Config.mouseKeyInverval;
        _截图直接复制到剪贴板 = ConfigManger.Config.截图直接复制到剪贴板;
        _isInitializin = false;
    }

    partial void OnMouseKeyChanged(MouseHookType value)
    {
        if (_isInitializin)
        {
            return;
        }

        ConfigManger.Config.mouseKey = value;
        ConfigManger.Save();
    }


    partial void OnThemeChoiceChanged(string value)
    {
        if (_isInitializin)
        {
            return;
        }

        switch (value)
        {
            case "跟随系统":
            {
                ServiceManager.Services.GetService<IThemeChange>().followSys(true);
                break;
            }
            case "深色":
            {
                ServiceManager.Services.GetService<IThemeChange>().followSys(false);
                ServiceManager.Services.GetService<IThemeChange>().changeTo("theme_dark");
                break;
            }
            case "浅色":
            {
                ServiceManager.Services.GetService<IThemeChange>().followSys(false);
                ServiceManager.Services.GetService<IThemeChange>().changeTo("theme_light");
                break;
            }
        }

        ConfigManger.Config.themeChoice = value;
        ConfigManger.Save();
    }

    [RelayCommand]
    private async Task DelKey(string key) =>
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            IgnoreItems.Remove(key);
            // IgnoreItems.ResetBindings();
            ((SearchWindowViewModel)ServiceManager.Services.GetService(typeof(SearchWindowViewModel)))
                .AddCollection(key);
            ConfigManger.Save();
        });
    //ConfigManger.Config.ignoreItems.Remove(key);

    partial void OnInputSmoothingMillisecondsChanged(int value)
    {
        if (_isInitializin)
        {
            return;
        }

        ConfigManger.Config.inputSmoothingMilliseconds = value;
        ConfigManger.Save();
    }

    partial void OnMouseKeyInvervalChanged(int value)
    {
        if (_isInitializin)
        {
            return;
        }

        ConfigManger.Config.mouseKeyInverval = value;
        ConfigManger.Save();
    }


    partial void OnMaxHistoryChanged(int value)
    {
        if (_isInitializin)
        {
            return;
        }

        ConfigManger.Config.maxHistory = value;
        ConfigManger.Save();
    }

    partial void OnCanReadClipboardChanged(bool value)
    {
        if (_isInitializin)
        {
            return;
        }

        ConfigManger.Config.canReadClipboard = value;
        ConfigManger.Save();
    }

    partial void OnAutoStartEverythingChanged(bool value)
    {
        if (_isInitializin)
        {
            return;
        }

        ConfigManger.Config.autoStartEverything = value;
        ConfigManger.Save();
    }

    partial void On截图直接复制到剪贴板Changed(bool value)
    {
        if (_isInitializin)
        {
            return;
        }

        ConfigManger.Config.截图直接复制到剪贴板 = value;
        ConfigManger.Save();
    }

    partial void OnAutoStartChanged(bool value)
    {
        if (_isInitializin)
        {
            return;
        }

        ConfigManger.Config.autoStart = value;
        ConfigManger.Save();
        if (value)
        {
            var strName = AppDomain.CurrentDomain.BaseDirectory + "Kitopia.exe"; //获取要自动运行的应用程序名
            if (!File.Exists(strName)) //判断要自动运行的应用程序文件是否存在
            {
                return;
            }

            var registry =
                Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run", true); //检索指定的子项
            if (registry == null) //若指定的子项不存在
            {
                registry = Registry.CurrentUser.CreateSubKey(
                    "Software\\Microsoft\\Windows\\CurrentVersion\\Run"); //则创建指定的子项
            }

            log.Info("用户确认启用开机自启");
            try
            {
                registry.SetValue("Kitopia", $"\"{strName}\""); //设置该子项的新的“键值对”
                ((IToastService)ServiceManager.Services.GetService(typeof(IToastService))).Show("开机自启", "开机自启设置成功");
            }
            catch (Exception exception)
            {
                log.Error("开机自启设置失败");
                log.Error(exception.StackTrace);
                ((IToastService)ServiceManager.Services.GetService(typeof(IToastService))).Show("开机自启", "开机自启设置失败");
            }
        }
        else
        {
            try
            {
                var registry =
                    Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run",
                        true); //检索指定的子项
                registry?.DeleteValue("Kitopia");
            }
            catch (Exception)
            {
            }
        }
    }

    partial void OnUseEverythingChanged(bool value)
    {
        if (_isInitializin)
        {
            return;
        }

        ConfigManger.Config.useEverything = value;
        ((SearchWindowViewModel)ServiceManager.Services.GetService(typeof(SearchWindowViewModel))).EverythingIsOk =
            !value;
        ConfigManger.Save();
    }
}