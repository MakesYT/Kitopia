#region

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Core.SDKs.HotKey;
using Core.SDKs.Services;
using Core.SDKs.Services.Config;
using log4net;
using Microsoft.Win32;

#endregion

namespace Core.ViewModel.Pages;

public partial class SettingPageViewModel : ObservableRecipient
{
    private static readonly ILog log = LogManager.GetLogger("SettingPageViewModel");

    [ObservableProperty] private bool _autoStart = true;
    [ObservableProperty] private bool _autoStartEverything = true;
    [ObservableProperty] private bool _canReadClipboard = true;
    [ObservableProperty] private BindingList<string> _ignoreItems;
    [ObservableProperty] private int _inputSmoothingMilliseconds = 50;
    private bool _isInitializing = false;
    [ObservableProperty] private int _maxHistory = 4;

    [ObservableProperty] private ObservableCollection<int> _maxHistoryOptions = new() { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
    [ObservableProperty] private string _themeChoice = "跟随系统";

    [ObservableProperty] private ObservableCollection<string> _themeChoiceOptions = new() { "跟随系统", "深色", "浅色" };
    [ObservableProperty] private bool _useEverything = true;

    public SettingPageViewModel()
    {
        Application.Current.Dispatcher.BeginInvoke(() =>
        {
            ThemeChoice = ConfigManger.Config.themeChoice;
            AutoStart = ConfigManger.Config.autoStart;
            AutoStartEverything = ConfigManger.Config.autoStartEverything;
            UseEverything = ConfigManger.Config.useEverything;
            MaxHistory = ConfigManger.Config.maxHistory;
            CanReadClipboard = ConfigManger.Config.canReadClipboard;
            IgnoreItems = new BindingList<string>(ConfigManger.Config.ignoreItems);
            InputSmoothingMilliseconds = ConfigManger.Config.inputSmoothingMilliseconds;

            _isInitializing = false;
        });
    }


    partial void OnThemeChoiceChanged(string value)
    {
        if (_isInitializing)
        {
            return;
        }

        switch (value)
        {
            case "跟随系统":
            {
                ((IThemeChange)ServiceManager.Services.GetService(typeof(IThemeChange))).followSys(true);
                break;
            }
            case "深色":
            {
                ((IThemeChange)ServiceManager.Services.GetService(typeof(IThemeChange))).followSys(false);
                ((IThemeChange)ServiceManager.Services.GetService(typeof(IThemeChange))).changeTo("theme_dark");
                break;
            }
            case "浅色":
            {
                ((IThemeChange)ServiceManager.Services.GetService(typeof(IThemeChange))).followSys(false);
                ((IThemeChange)ServiceManager.Services.GetService(typeof(IThemeChange))).changeTo("theme_light");
                break;
            }
        }

        ConfigManger.Config.themeChoice = value;
        ConfigManger.Save();
    }

    [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
    public static extern IntPtr GetForegroundWindow();

    [RelayCommand]
    private void EditHotKey(string name)
    {
        var hwndSource = System.Windows.Interop.HwndSource.FromHwnd(GetForegroundWindow());
        if (hwndSource == null)
        {
            return;
        }

        var xx = (Window)hwndSource.RootVisual;
        var hotKeyModel = ConfigManger.Config.hotKeys.FirstOrDefault(e =>
        {
            if ($"{e.MainName}_{e.Name}".Equals(name))
            {
                return true;
            }

            return false;
        });
        if (hotKeyModel is null)
        {
            var strings = name.Split("_", 2);
            var hotKeyModel2 = new HotKeyModel()
                { MainName = strings[0], Name = strings[1], IsUsable = true };
            ConfigManger.Config.hotKeys.Add(hotKeyModel2);
            ConfigManger.Save();
            ((IHotKeyEditor)ServiceManager.Services.GetService(typeof(IHotKeyEditor))!).EditByName(name, xx);
        }
        else
        {
            ((IHotKeyEditor)ServiceManager.Services.GetService(typeof(IHotKeyEditor))!).EditByHotKeyModel(hotKeyModel,
                xx);
        }
    }

    [RelayCommand]
    private async Task DelKey(string key) =>
        await Application.Current.Dispatcher.BeginInvoke(() =>
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
        if (_isInitializing)
        {
            return;
        }

        ConfigManger.Config.inputSmoothingMilliseconds = value;
        ConfigManger.Save();
    }


    partial void OnMaxHistoryChanged(int value)
    {
        if (_isInitializing)
        {
            return;
        }

        ConfigManger.Config.maxHistory = value;
        ConfigManger.Save();
    }

    partial void OnCanReadClipboardChanged(bool value)
    {
        if (_isInitializing)
        {
            return;
        }

        ConfigManger.Config.canReadClipboard = value;
        ConfigManger.Save();
    }

    partial void OnAutoStartEverythingChanged(bool value)
    {
        if (_isInitializing)
        {
            return;
        }

        ConfigManger.Config.autoStartEverything = value;
        ConfigManger.Save();
    }


    partial void OnAutoStartChanged(bool value)
    {
        if (_isInitializing)
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
                ((IToastService)ServiceManager.Services.GetService(typeof(IToastService))).Show("开机自启设置成功");
            }
            catch (Exception exception)
            {
                log.Error("开机自启设置失败");
                log.Error(exception.StackTrace);
                ((IToastService)ServiceManager.Services.GetService(typeof(IToastService))).Show("开机自启设置失败");
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
        if (_isInitializing)
        {
            return;
        }

        ConfigManger.Config.useEverything = value;
        ((SearchWindowViewModel)ServiceManager.Services.GetService(typeof(SearchWindowViewModel))).EverythingIsOk =
            !value;
        ConfigManger.Save();
    }
}