﻿using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Core.SDKs.Config;
using Core.SDKs.HotKey;
using Core.SDKs.Services;
using log4net;
using Microsoft.Win32;

namespace Core.ViewModel.Pages;

public partial class SettingPageViewModel : ObservableRecipient
{
    private static readonly ILog log = LogManager.GetLogger("SettingPageViewModel");

    [ObservableProperty]
    private IList<int> _maxHistoryOptions = new ObservableCollection<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

    [ObservableProperty]
    private IList<string> _themeChoiceOptions = new ObservableCollection<string> { "跟随系统", "深色", "浅色" };

    [ObservableProperty] public bool autoStart = true;
    [ObservableProperty] public bool canReadClipboard = true;
    [ObservableProperty] public int inputSmoothingMilliseconds = 50;
    [ObservableProperty] public int maxHistory = 4;
    [ObservableProperty] public string themeChoice = "跟随系统";
    [ObservableProperty] public bool useEverything = true;
    [ObservableProperty] public bool debugMode = false;
    [ObservableProperty] public BindingList<HotKeyModel> hotKeys;

    public SettingPageViewModel()
    {
        ThemeChoice = ConfigManger.config.themeChoice;
        AutoStart = ConfigManger.config.autoStart;
        UseEverything = ConfigManger.config.useEverything;
        MaxHistory = ConfigManger.config.maxHistory;
        CanReadClipboard = ConfigManger.config.canReadClipboard;
        DebugMode = ConfigManger.config.debugMode;
        HotKeys = ConfigManger.config.hotKeys;
        WeakReferenceMessenger.Default.Register<string, string>(this, "hotkey", (r, m) =>
        {
            HotKeys = ConfigManger.config.hotKeys;
            OnPropertyChanged(nameof(HotKeys));
        });
    }


    partial void OnThemeChoiceChanged(string value)
    {
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

        ConfigManger.config.themeChoice = value;
        ConfigManger.Save();
    }

    partial void OnInputSmoothingMillisecondsChanged(int value)
    {
        ConfigManger.config.inputSmoothingMilliseconds = value;
        ConfigManger.Save();
    }

    partial void OnDebugModeChanged(bool value)
    {
        ConfigManger.config.debugMode = value;
        ConfigManger.Save();
    }


    partial void OnMaxHistoryChanged(int value)
    {
        ConfigManger.config.maxHistory = value;
        ConfigManger.Save();
    }

    partial void OnCanReadClipboardChanged(bool value)
    {
        ConfigManger.config.canReadClipboard = value;
        ConfigManger.Save();
    }

    partial void OnAutoStartChanged(bool value)
    {
        ConfigManger.config.autoStart = value;
        ConfigManger.Save();
        if (value)
        {
            string strName = AppDomain.CurrentDomain.BaseDirectory + "Kitopia.exe"; //获取要自动运行的应用程序名
            if (!System.IO.File.Exists(strName)) //判断要自动运行的应用程序文件是否存在
                return;
            RegistryKey registry =
                Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run", true); //检索指定的子项
            if (registry == null) //若指定的子项不存在
                registry = Registry.CurrentUser.CreateSubKey(
                    "Software\\Microsoft\\Windows\\CurrentVersion\\Run"); //则创建指定的子项
            log.Info("用户确认启用开机自启");
            try
            {
                registry.SetValue("Kitopia", $"\"{strName}\""); //设置该子项的新的“键值对”
                ((IToastService)ServiceManager.Services.GetService(typeof(IToastService))).show("开机自启设置成功");
            }
            catch (Exception exception)
            {
                log.Error("开机自启设置失败");
                log.Error(exception.StackTrace);
                ((IToastService)ServiceManager.Services.GetService(typeof(IToastService))).show("开机自启设置失败");
            }
        }
        else
        {
            try
            {
                RegistryKey registry =
                    Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run",
                        true); //检索指定的子项
                registry?.DeleteValue("Kitopia");
            }
            catch (Exception e)
            {
            }
        }
    }

    partial void OnUseEverythingChanged(bool value)
    {
        ConfigManger.config.useEverything = value;
        ((SearchWindowViewModel)ServiceManager.Services.GetService(typeof(SearchWindowViewModel))).EverythingIsOk =
            !value;
        ConfigManger.Save();
    }
}