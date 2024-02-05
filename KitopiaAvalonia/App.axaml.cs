using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Core.SDKs;
using Core.SDKs.CustomScenario;
using Core.SDKs.Services;
using Core.SDKs.Services.Config;
using Core.SDKs.Services.Plugin;
using Core.SDKs.Tools;
using Core.ViewModel;
using Core.ViewModel.Pages;
using Core.ViewModel.Pages.customScenario;
using Core.ViewModel.Pages.plugin;
using Core.ViewModel.TaskEditor;
using Kitopia.Services;
using KitopiaAvalonia.Pages;
using KitopiaAvalonia.Services;
using KitopiaAvalonia.Windows;
using log4net;
using log4net.Config;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using PluginCore;

namespace KitopiaAvalonia;

public partial class App : Application
{
    private static readonly ILog log = LogManager.GetLogger("App");

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);

        if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var desktop1 = desktop.MainWindow;
        }

        Console.WriteLine(1);
    }


    public override void OnFrameworkInitializationCompleted()
    {
        ServiceManager.Services = ConfigureServices();
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = ServiceManager.Services.GetService<MainWindow>();
            ServiceManager.Services.GetService<IToastService>().Init(desktop.MainWindow);
            DataContext = new AppViewModel();
            OnStartup();
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void OnStartup()
    {
        var eventWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset, "Kitopia", out var createNew);
        if (!createNew)
        {
            var content = new DialogContent("Kitopia", "不能同时开启两个应用", null, null, "确定", () =>
            {
                Environment.Exit(0);
            }, null, null);
            ServiceManager.Services.GetService<IContentDialog>()!.ShowDialog(null, content);
        }
        else
        {
            var assembly = Assembly.GetExecutingAssembly();
            var logConfigStream = assembly.GetManifestResourceStream("KitopiaAvalonia.log4net.config")!;

            XmlConfigurator.Configure(logConfigStream);
#if !DEBUG
            log.Info("异常捕获");
            // DispatcherUnhandledException += App_DispatcherUnhandledException;
            // Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
            // AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
#endif
            CheckAndDeleteLogFiles();
            log.Info("启动");
            ThreadPool.RegisterWaitForSingleObject(eventWaitHandle, (_, _) =>
            {
                // Dispatcher.Invoke(() =>
                // {
                //     ServiceManager.Services.GetService<SearchWindow>()!.Show();
                //     User32.SetForegroundWindow(
                //         new WindowInteropHelper(ServiceManager.Services.GetService<SearchWindow>()!)
                //             .Handle);
                //
                //     ServiceManager.Services.GetService<SearchWindow>()!.tx.Focus();
                // });
            }, null, -1, false);

            log.Info("Ioc初始化完成");
            ConfigManger.Init();
            log.Info("配置文件初始化完成");
            PluginManager.Init();
            log.Info("插件管理器初始化完成");
            CustomScenarioManger.Init();
            log.Info("场景管理器初始化完成");
            switch (ConfigManger.Config.themeChoice)
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

            log.Info("主题初始化完成");
            log.Debug("注册热键");
            ServiceManager.Services.GetService<MainWindow>().InitHook();
            SystemEvents.InvokeOnEventsThread(() =>
            {
                // MouseHookHelper.InsertMouseHook();
            });


            ServicePointManager.DefaultConnectionLimit = 10240;

            if (ConfigManger.Config.autoStart)
            {
                log.Info("设置开机自启");
                SetAutoStartup();
            }
        }
    }

    private static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IconTools>();
        services.AddSingleton<IToastService, ToastService>();
        services.AddTransient<IContentDialog, ContentDialogService>();
        services.AddTransient<IHotKeyEditor, HotKeyEditorService>();
        services.AddSingleton<ITaskEditorOpenService, TaskEditorOpenService>();
        services.AddTransient<IClipboardService, ClipboardService>();
        services.AddTransient<IThemeChange, ThemeChange>();
        services.AddSingleton<ISearchItemTool, SearchItemTool>();
        services.AddTransient<TaskEditorViewModel>();
        services.AddTransient<TaskEditor>(e => new TaskEditor() { DataContext = e.GetService<TaskEditorViewModel>() });
        services.AddSingleton<MainWindowViewModel>();
        services.AddSingleton<MainWindow>(e => new MainWindow() { DataContext = e.GetService<MainWindowViewModel>() });
        services.AddSingleton<SearchWindowViewModel>(e => new SearchWindowViewModel() { IsActive = true });
        services.AddSingleton<SearchWindow>(e => new SearchWindow()
            { DataContext = e.GetService<SearchWindowViewModel>() });
        services.AddSingleton<SettingPageViewModel>(e => new SettingPageViewModel() { IsActive = true });
        services.AddKeyedSingleton<UserControl, SettingPage>("SettingPage",
            (e, _) => new SettingPage() { DataContext = e.GetService<SettingPageViewModel>() });
        services.AddSingleton<HomePageViewModel>(e => new HomePageViewModel() { IsActive = true });
        services.AddKeyedSingleton<UserControl, HomePage>("HomePage",
            (e, _) => new HomePage() { DataContext = e.GetService<HomePageViewModel>() });
        services.AddSingleton<CustomScenariosManagerPageViewModel>(e => new CustomScenariosManagerPageViewModel()
            { IsActive = true });
        services.AddKeyedSingleton<UserControl, CustomScenariosManagerPage>("CustomScenariosManagerPage",
            (e, _) => new CustomScenariosManagerPage()
                { DataContext = e.GetService<CustomScenariosManagerPageViewModel>() });
        services.AddSingleton<HotKeyManagerPageViewModel>(e => new HotKeyManagerPageViewModel() { });
        services.AddKeyedSingleton<UserControl, HotKeyManagerPage>("HotKeyManagerPage",
            (e, _) => new HotKeyManagerPage() { DataContext = e.GetService<HotKeyManagerPageViewModel>() });
        services.AddSingleton<PluginManagerPageViewModel>(e => new PluginManagerPageViewModel() { IsActive = true });
        services.AddKeyedSingleton<UserControl, PluginManagerPage>("PluginManagerPage",
            (e, _) => new PluginManagerPage() { DataContext = e.GetService<PluginManagerPageViewModel>() });

        return services.BuildServiceProvider();
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
        var strName = AppDomain.CurrentDomain.BaseDirectory + "KitopiaAvalonia.exe"; //获取要自动运行的应用程序名
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

        if (registry.GetValue("Kitopia") is null)
        {
            ServiceManager.Services.GetService<IContentDialog>().ShowDialogAsync(null, new DialogContent("Kitopia",
                new TextBlock()
                {
                    Text = "是否设置开机自启?\n可能被杀毒软件阻止      ",
                    FontSize = 15
                }, "取消", null, "确定", () =>
                {
                    log.Info("用户确认启用开机自启");
                    try
                    {
                        registry.SetValue("Kitopia", $"\"{strName}\""); //设置该子项的新的“键值对”
                        ((IToastService)ServiceManager.Services.GetService(typeof(IToastService))!).Show("开机自启",
                            "开机自启设置成功");
                    }
                    catch (Exception exception)
                    {
                        log.Error("开机自启设置失败");
                        log.Error(exception.StackTrace);
                        ((IToastService)ServiceManager.Services.GetService(typeof(IToastService))!).Show("开机自启",
                            "开机自启设置失败,请检查杀毒软件后重试");
                    }
                }, null, () =>
                {
                    log.Info("用户取消启用开机自启");
                }));
        }
        else if (!registry.GetValue("Kitopia").Equals($"\"{strName}\""))
        {
            try
            {
                registry.SetValue("Kitopia", $"\"{strName}\""); //设置该子项的新的“键值对”
                ((IToastService)ServiceManager.Services.GetService(typeof(IToastService))!).Show("开机自启", "开机自启设置成功");
            }
            catch (Exception exception)
            {
                log.Error("开机自启设置失败");
                log.Error(exception.StackTrace);
                ((IToastService)ServiceManager.Services.GetService(typeof(IToastService))!).Show("开机自启",
                    "开机自启设置失败,请检查杀毒软件后重试");
            }
        }
        else
        {
            log.Debug("程序自启已存在");
        }
    }
}