using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using Core.SDKs.CustomScenario;
using Core.SDKs.Services;
using Core.SDKs.Services.Config;
using Core.SDKs.Services.Plugin;
using Core.ViewModel;
using log4net;
using Microsoft.Extensions.DependencyInjection;
using Ursa.Controls;
using HotKeyManager = Core.SDKs.HotKey.HotKeyManager;

namespace KitopiaAvalonia;

public partial class MainWindow : UrsaWindow
{
    private static readonly ILog log = LogManager.GetLogger(nameof(MainWindow));

    public MainWindow()
    {
        InitializeComponent();
        Task.Run(() => { OnStartup(); });
        Dispatcher.UIThread.UnhandledException += (sender, e) =>
        {
            e.Handled = true;
            log.Fatal(e.Exception);
        };


        IsVisible = false;
    }


    private void Window_OnClosing(object? sender, WindowClosingEventArgs e)
    {
        this.IsVisible = false;
        e.Cancel = true;
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


        HotKeyManager.Init();
        log.Debug("注册热键管理器完成");
        ConfigManger.Init();
        log.Info("配置文件初始化完成");
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

        PluginManager.Init();
        log.Info("插件管理器初始化完成");
        CustomScenarioManger.Init();
        log.Info("场景管理器初始化完成");


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

    private void TitleBarHost_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        BeginMoveDrag(e);
    }
}