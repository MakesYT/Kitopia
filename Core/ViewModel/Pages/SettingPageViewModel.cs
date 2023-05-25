using System.Collections.ObjectModel;
using System.Reflection.PortableExecutable;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Core.SDKs.Config;
using Core.SDKs.Services;
using log4net;
using Microsoft.Win32;

namespace Core.ViewModel.Pages;

public partial class SettingPageViewModel: ObservableRecipient
{
    private static readonly ILog log = LogManager.GetLogger("SettingPageViewModel");
    public SettingPageViewModel()
    {
        CurrentThemeIsDark = ConfigManger.config.isDark;
        ThemeFollowSys = ConfigManger.config.themeFollowSys;
        AutoStart =ConfigManger.config.autoStart;
        UseEverything = ConfigManger.config.useEverything;
        MaxHistory = ConfigManger.config.maxHistory;
        CanReadClipboard = ConfigManger.config.canReadClipboard;
        WeakReferenceMessenger.Default.Register<string,string>(this,"themeChange", (r, m) =>
        {
            CurrentThemeIsDark = ((IThemeChange)ServiceManager.Services.GetService(typeof(IThemeChange))).isDark();
        });
        
    }
    [ObservableProperty] public bool currentThemeIsDark = false;
    [ObservableProperty] public bool themeFollowSys = false;
    [ObservableProperty] public bool autoStart = true;
    [ObservableProperty] public bool useEverything = true;
    [ObservableProperty] public bool canReadClipboard = true;
    [ObservableProperty] public int maxHistory = 4;
    [ObservableProperty]
    private IList<int> _maxHistoryOptions = new ObservableCollection<int>{1,2,3,4,5,6,7,8,9,10};
    [RelayCommand]
    public void changeTheme(string name)
    {
        ((IThemeChange)ServiceManager.Services.GetService(typeof(IThemeChange))).changeTo(name);
        CurrentThemeIsDark = ((IThemeChange)ServiceManager.Services.GetService(typeof(IThemeChange))).isDark();
        ConfigManger.config.isDark = CurrentThemeIsDark;
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

    
    
    partial void OnThemeFollowSysChanged(bool value)
    {
        ((IThemeChange)ServiceManager.Services.GetService(typeof(IThemeChange))).followSys(value);
        CurrentThemeIsDark = ((IThemeChange)ServiceManager.Services.GetService(typeof(IThemeChange))).isDark();
    }

    partial void OnAutoStartChanged(bool value)
    {
        ConfigManger.config.autoStart = value;
        ConfigManger.Save();
        if (value)
        {
            string strName = AppDomain.CurrentDomain.BaseDirectory + "Kitopia.exe";//获取要自动运行的应用程序名
            if (!System.IO.File.Exists(strName))//判断要自动运行的应用程序文件是否存在
                return;
            RegistryKey registry = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run", true);//检索指定的子项
            if (registry == null)//若指定的子项不存在
                registry = Registry.CurrentUser.CreateSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run");//则创建指定的子项
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
            RegistryKey registry = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run", true);//检索指定的子项
            registry?.DeleteValue("Kitopia");
        }
    }

    partial void OnUseEverythingChanged(bool value)
    {
        ConfigManger.config.useEverything = value;
        ((SearchWindowViewModel)ServiceManager.Services.GetService(typeof(SearchWindowViewModel))).EverythingIsOk = !value;
        ConfigManger.Save();
    }
}