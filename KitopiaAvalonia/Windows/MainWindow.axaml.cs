using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Core.SDKs.CustomScenario;
using Core.SDKs.Services;
using Core.SDKs.Services.Config;
using Core.SDKs.Services.Plugin;
using Core.ViewModel;
using Core.ViewModel.Pages.plugin;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Navigation;
using FluentAvalonia.UI.Windowing;
using KitopiaAvalonia.Pages;
using log4net;
using Microsoft.Extensions.DependencyInjection;
using HotKeyManager = Core.SDKs.HotKey.HotKeyManager;

namespace KitopiaAvalonia;

public class NavigationPageFactory : INavigationPageFactory
{
    public Control GetPage(Type srcType) => throw new NotImplementedException();

    public Control GetPageFromObject(object target)
    {
        if (target is string s)
        {
            if (s == "SettingPage")
            {
                var settingPage = ServiceManager.Services.GetService<SettingPage>();
                settingPage.ChangeConfig(ConfigManger.Config);
                return settingPage;
            }

            if (s.StartsWith("PluginSettingSelectPage_"))
            {
                var keyedService = ServiceManager.Services.GetKeyedService<UserControl>("PluginSettingSelectPage");
                ((PluginSettingViewModel)keyedService.DataContext).LoadByPluginInfo(s.Split("_", 2)[1]);
                return keyedService;
            }

            if (s.StartsWith("PluginSetting_"))
            {
                var settingPage = ServiceManager.Services.GetService<SettingPage>();
                if (ConfigManger.Configs.TryGetValue(s.Split("_", 2)[1], out var config))
                {
                    settingPage.ChangeConfig(config);
                }

                return settingPage;
            }

            return ServiceManager.Services.GetKeyedService<UserControl>(s);
        }

        return null;
    }
}

public partial class MainWindow : AppWindow
{
    private static readonly ILog log = LogManager.GetLogger(nameof(MainWindow));

    public MainWindow()
    {
        InitializeComponent();
        Task.Run(() => { OnStartup(); });
        Dispatcher.UIThread.UnhandledException += (sender, e) => {
            e.Handled = true;
            log.Fatal(e.Exception);
        };
        RenderOptions.SetTextRenderingMode(this, TextRenderingMode.Antialias);
        TitleBar.ExtendsContentIntoTitleBar = true;
        TitleBar.TitleBarHitTestType = TitleBarHitTestType.Complex;
        FrameView.NavigationPageFactory = new NavigationPageFactory();

        foreach (NavigationViewItem nvi in NavView.MenuItems)
        {
            if (nvi.Tag == FrameView.Tag)
            {
                NavView.SelectedItem = nvi;
                SetNVIIcon(nvi, true);
            }
            else
            {
                SetNVIIcon(nvi, false);
            }
        }

        foreach (NavigationViewItem nvi in NavView.FooterMenuItems)
        {
            if (nvi.Tag == FrameView.Tag)
            {
                NavView.SelectedItem = nvi;
                SetNVIIcon(nvi, true);
            }
            else
            {
                SetNVIIcon(nvi, false);
            }
        }

        IsVisible = false;
    }

    private IReadOnlyDictionary<Type, string> Pages => new Dictionary<Type, string>
    {
        { typeof(HomePage), "HomePage" },
        { typeof(PluginManagerPage), "PluginManagerPage" },
        { typeof(CustomScenariosManagerPage), "CustomScenariosManagerPage" },
        { typeof(HotKeyManagerPage), "HotKeyManagerPage" },
        { typeof(SettingPage), "SettingPage" }
    };


    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        if (VisualRoot is AppWindow aw)
        {
            TitleBarHost.ColumnDefinitions[3].Width = new GridLength(aw.TitleBar.RightInset, GridUnitType.Pixel);
        }
    }

    private void Window_OnClosing(object? sender, WindowClosingEventArgs e)
    {
        this.IsVisible = false;
        e.Cancel = true;
    }

    private void NavView_OnItemInvoked(object? sender, NavigationViewItemInvokedEventArgs e)
    {
        FrameView.NavigateFromObject(e.InvokedItemContainer.Tag);
    }

    protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnAttachedToLogicalTree(e);
    }

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
    }

    private void FirstOpenEventHandler(object? o, EventArgs args)
    {
        Dispatcher.UIThread.InvokeAsync(() => { this.IsVisible = false; });
        this.Opened -= FirstOpenEventHandler;
    }

    public void OnStartup()
    {
        log.Info("启动");
        this.Opened += FirstOpenEventHandler;

        CheckAndDeleteLogFiles();
        log.Info("启动");


        log.Info("Ioc初始化完成");
        ConfigManger.Init();
        log.Info("配置文件初始化完成");
        PluginManager.Init();
        log.Info("插件管理器初始化完成");
        CustomScenarioManger.Init();
        log.Info("场景管理器初始化完成");
        switch (ConfigManger.Config.themeChoice)
        {
            case ThemeEnum.跟随系统:
            {
                ServiceManager.Services.GetService<IThemeChange>()
                              .followSys(true);
                break;
            }
            case ThemeEnum.深色:
            {
                ServiceManager.Services.GetService<IThemeChange>()
                              .followSys(false);
                ServiceManager.Services.GetService<IThemeChange>()
                              .changeTo("theme_dark");
                break;
            }
            case ThemeEnum.浅色:
            {
                ServiceManager.Services.GetService<IThemeChange>()
                              .followSys(false);
                ServiceManager.Services.GetService<IThemeChange>()
                              .changeTo("theme_light");
                break;
            }
        }

        log.Info("主题初始化完成");
        log.Debug("注册热键");
        HotKeyManager.Init();


        ServicePointManager.DefaultConnectionLimit = 10240;

        if (ConfigManger.Config.autoStart)
        {
            log.Info("设置开机自启");
            SetAutoStartup();
        }

        Dispatcher.UIThread.InvokeAsync(() => { ServiceManager.Services.GetService<SearchWindowViewModel>(); });
    }


    private static void CheckAndDeleteLogFiles()
    {
        // 定义日志文件的目录
        var logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
        log.Debug($"检查日志目录:{logDirectory}");
        // 定义要保留的日志文件的时间范围，这里是一周
        var timeSpan = TimeSpan.FromDays(2);

        // 获取当前的日期
        var currentDate = DateTime.Today;

        // 获取目录下的所有日志文件，按照最后修改时间排序
        var logFiles = Directory.EnumerateFiles(logDirectory)
                                .Select(f => new FileInfo(f))
                                .OrderByDescending(f => f.LastWriteTime);

        // 遍历每个日志文件
        foreach (var logFile in logFiles)
        {
            // 计算日志文件的最后修改时间和当前日期的差值
            // 如果差值大于要保留的时间范围，就删除该日志文件
            if (currentDate - logFile.LastWriteTime > timeSpan)
            {
                log.Debug($"删除日志文件:{logFile.FullName}");
                logFile.Delete();
            }
        }
    }

    private void SetAutoStartup()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            SetAutoStartupWindow();
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            SetAutoStartupLinux();
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            throw new NotImplementedException();
        }
    }

    private void SetAutoStartupWindow()
    {
        ServiceManager.Services.GetService<IAutoStartService>()
                      .ChangeAutoStart(true);
    }

    private void SetAutoStartupLinux()
    {
        var strName = AppDomain.CurrentDomain.BaseDirectory + "KitopiaAvalonia";
        if (!File.Exists(strName)) //判断要自动运行的应用程序文件是否存在
        {
            return;
        }

        var iniF = $"{Environment.GetEnvironmentVariable("HOME")}/.config/autostart/kitopia.desktop";
        if (File.Exists(iniF)) //判断要自动运行的应用程序文件是否存在
        {
            return;
        }

        // \nIcon=/home/makesyt/.local/share/JetBrains/Toolbox/toolbox.svg
        File.WriteAllText(iniF,
            $"[Desktop Entry]\nExec={strName}\nVersion=1.0\nType=Application\nCategories=Development\nName=Kitopia\nStartupWMClass=kitopia\nTerminal=false\nX-GNOME-Autostart-enabled=true\nStartupNotify=false\nX-GNOME-Autostart-Delay=10\nX-MATE-Autostart-Delay=10\nX-KDE-autostart-after=panel\nX-Deepin-CreatedBy=com.deepin.SessionManager\nX-Deepin-AppID=kitopia\nHidden=false");
    }

    private void FrameView_OnNavigated(object sender, NavigationEventArgs e)
    {
        FrameView.Tag = Pages.GetValueOrDefault(e.Content.GetType());
        foreach (NavigationViewItem nvi in NavView.MenuItems)
        {
            if (nvi.Tag == FrameView.Tag)
            {
                NavView.SelectedItem = nvi;
                SetNVIIcon(nvi, true);
            }
            else
            {
                SetNVIIcon(nvi, false);
            }
        }

        foreach (NavigationViewItem nvi in NavView.FooterMenuItems)
        {
            if (nvi.Tag == FrameView.Tag)
            {
                NavView.SelectedItem = nvi;
                SetNVIIcon(nvi, true);
            }
            else
            {
                SetNVIIcon(nvi, false);
            }
        }

        if (FrameView.BackStackDepth > 0 && !NavView.IsBackButtonVisible)
        {
            AnimateContentForBackButton(true);
        }
        else if (FrameView.BackStackDepth == 0 && NavView.IsBackButtonVisible)
        {
            AnimateContentForBackButton(false);
        }
    }

    private void SetNVIIcon(NavigationViewItem item, bool selected)
    {
        // Technically, yes you could set up binding and converters and whatnot to let the icon change
        // between filled and unfilled based on selection, but this is so much simpler 

        if (item == null)
            return;

        var t = item.Tag;
        switch (t)
        {
            case "HomePage":
            {
                item.IconSource = this.TryFindResource(selected ? "HomeIconFilled" : "HomeIcon", out var value)
                    ? (IconSource)value
                    : null;
                break;
            }
            case "PluginManagerPage":
            {
                item.IconSource = this.TryFindResource(selected ? "AppsIconFilled" : "AppsIcon", out var value)
                    ? (IconSource)value
                    : null;
                break;
            }
            case "CustomScenariosManagerPage":
            {
                item.IconSource =
                    this.TryFindResource(selected ? "AppListDetailIconFilled" : "AppListDetailIcon", out var value)
                        ? (IconSource)value
                        : null;
                break;
            }
            case "HotKeyManagerPage":
            {
                item.IconSource = this.TryFindResource(selected ? "KeyboardIconFilled" : "KeyboardIcon", out var value)
                    ? (IconSource)value
                    : null;
                break;
            }
            case "SettingPage":
            {
                item.IconSource = this.TryFindResource(selected ? "SettingsIconFilled" : "SettingsIcon", out var value)
                    ? (IconSource)value
                    : null;
                break;
            }
        }
    }

    private async void AnimateContentForBackButton(bool show)
    {
        if (!WindowIcon.IsVisible)
            return;

        if (show)
        {
            var ani = new Animation
            {
                Duration = TimeSpan.FromMilliseconds(250),
                FillMode = FillMode.Forward,
                Children =
                {
                    new KeyFrame
                    {
                        Cue = new Cue(0d),
                        Setters =
                        {
                            new Setter(MarginProperty, new Thickness(12, 4, 12, 4))
                        }
                    },
                    new KeyFrame
                    {
                        Cue = new Cue(1d),
                        KeySpline = new KeySpline(0, 0, 0, 1),
                        Setters =
                        {
                            new Setter(MarginProperty, new Thickness(48, 4, 12, 4))
                        }
                    }
                }
            };

            await ani.RunAsync(WindowIcon);

            NavView.IsBackButtonVisible = true;
        }
        else
        {
            NavView.IsBackButtonVisible = false;

            var ani = new Animation
            {
                Duration = TimeSpan.FromMilliseconds(250),
                FillMode = FillMode.Forward,
                Children =
                {
                    new KeyFrame
                    {
                        Cue = new Cue(0d),
                        Setters =
                        {
                            new Setter(MarginProperty, new Thickness(48, 4, 12, 4))
                        }
                    },
                    new KeyFrame
                    {
                        Cue = new Cue(1d),
                        KeySpline = new KeySpline(0, 0, 0, 1),
                        Setters =
                        {
                            new Setter(MarginProperty, new Thickness(12, 4, 12, 4))
                        }
                    }
                }
            };

            await ani.RunAsync(WindowIcon);
        }
    }

    private void NavView_OnBackRequested(object? sender, NavigationViewBackRequestedEventArgs e)
    {
        FrameView.GoBack();
    }
}