using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;
using Core.SDKs.Services;
using Core.SDKs.Services.Config;
using Core.SDKs.Services.Plugin;
using Core.SDKs.Tools;
using Core.ViewModel;
using Core.ViewModel.Pages;
using Core.ViewModel.Pages.plugin;
using Core.ViewModel.TaskEditor;
using Kitopia.Services;
using Kitopia.View;
using Kitopia.View.Pages;
using Kitopia.View.Pages.Plugin;
using log4net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Uwp.Notifications;
using Microsoft.Win32;
using Wpf.Ui.Contracts;
using Wpf.Ui.Services;
using MessageBox = Kitopia.Controls.MessageBoxControl.MessageBox;
using MessageBoxResult = Kitopia.Controls.MessageBoxControl.MessageBoxResult;

namespace Kitopia;

/// <summary>
///     Interaction logic for App.xaml
/// </summary>
public sealed partial class App : Application
{
    private static readonly ILog log = LogManager.GetLogger("App");

    public static void CheckAndDeleteLogFiles()
    {
        // 定义日志文件的目录
        string logDirectory = AppDomain.CurrentDomain.BaseDirectory + "logs";
        log.Debug($"检查日志目录:{logDirectory}");
        // 定义要保留的日志文件的时间范围，这里是一周
        TimeSpan timeSpan = TimeSpan.FromDays(2);

        // 获取当前的日期
        DateTime currentDate = DateTime.Now;

        // 获取目录下的所有日志文件，按照最后修改时间排序
        var logFiles = Directory.GetFiles(logDirectory)
            .Select(f => new FileInfo(f))
            .OrderByDescending(f => f.LastWriteTime);

        // 遍历每个日志文件
        foreach (var logFile in logFiles)
        {
            // 计算日志文件的最后修改时间和当前日期的差值
            var diff = currentDate - logFile.LastWriteTime;

            // 如果差值大于要保留的时间范围，就删除该日志文件
            if (diff > timeSpan)
            {
                log.Debug($"删除日志文件:{logFile.FullName}");
                logFile.Delete();
            }
        }
    }

    [DllImport("user32.dll", EntryPoint = "SetForegroundWindow", SetLastError = true)]
    public static extern void SetForegroundWindow(IntPtr hwnd);

    protected override void OnStartup(StartupEventArgs e)
    {
        var eventWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset, "Kitopia", out var createNew);
        if (!createNew)
        {
            MessageBox msg = new MessageBox();
            msg.Title = "Kitopia";
            msg.Content = "不能同时开启两个应用        ";
            msg.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            msg.CloseButtonText = "确定";
            msg.FontSize = 15;
            var task = msg.ShowDialogAsync();
            // 使用ContinueWith来在任务完成后执行一个回调函数
            task.ContinueWith(e =>
            {
                MessageBoxResult result = e.Result;
            }).Wait();
            // System.Windows.MessageBox.Show("不能同时开启两个应用", "Kitopia");
            eventWaitHandle.Set();
            Environment.Exit(0);
        }
        else
        {
            log.Info("程序启动");
            ThreadPool.RegisterWaitForSingleObject(eventWaitHandle, (_, _) =>
            {
                Dispatcher.Invoke(() =>
                {
                    ServiceManager.Services.GetService<SearchWindow>().Show();
                    SetForegroundWindow(new WindowInteropHelper(ServiceManager.Services.GetService<SearchWindow>())
                        .Handle);

                    ServiceManager.Services.GetService<SearchWindow>().tx.Focus();
                });
            }, null, -1, false);
            log.Info("注册EventWaitHandle");
            var appReloadEventWaitHandle =
                new EventWaitHandle(false, EventResetMode.AutoReset, "Kitopia_appReload", out var appReload);
            if (!createNew)
            {
                MessageBox msg = new MessageBox();
                msg.Title = "Kitopia";
                msg.Content = "右键菜单捕获出现异常,\n请关闭软件重试,将会导致部分功能异常        ";
                msg.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                msg.CloseButtonText = "确定";
                msg.FontSize = 15;
                var task = msg.ShowDialogAsync();
                // 使用ContinueWith来在任务完成后执行一个回调函数
                task.ContinueWith(e =>
                {
                    MessageBoxResult result = e.Result;
                }).Wait();
                //System.Windows.MessageBox.Show("右键菜单捕获出现异常,请关闭软件重试,将会导致部分功能异常", "Kitopia");
            }
            else
            {
                ThreadPool.RegisterWaitForSingleObject(eventWaitHandle, (_, _) =>
                {
                    ServiceManager.Services.GetService<SearchWindowViewModel>().ReloadApps();
                }, null, -1, false);
            }

#if !DEBUG
            log.Info("异常捕获");
            DispatcherUnhandledException += App_DispatcherUnhandledException;
            Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
#endif
            CheckAndDeleteLogFiles();
            AppDomain.CurrentDomain.ProcessExit += Application_ApplicationExit;
            ServiceManager.Services = ConfigureServices();

            log.Info("Ioc初始化完成");
            ConfigManger.Init();
            log.Info("配置文件初始化完成");
            PluginManager.Init();
            log.Info("插件管理器初始化完成");

            var initWindow = ServiceManager.Services.GetService<MainWindow>();
            initWindow.Show();
            // initWindow.Close();

            Current.MainWindow = ServiceManager.Services.GetService<MainWindow>();
            ServicePointManager.DefaultConnectionLimit = 10240;

            if (ConfigManger.Config.autoStart)
            {
                log.Info("设置开机自启");
                SetAutoStartup();
            }

            base.OnStartup(e);
        }
    }

    static void Application_ApplicationExit(object? sender, EventArgs e)
    {
        ConfigManger.Save();
        log.Info("程序退出");
    }

    private void SetAutoStartup()
    {
        string strName = AppDomain.CurrentDomain.BaseDirectory + "Kitopia.exe"; //获取要自动运行的应用程序名
        if (!File.Exists(strName)) //判断要自动运行的应用程序文件是否存在
            return;
        RegistryKey registry =
            Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run", true); //检索指定的子项
        if (registry == null) //若指定的子项不存在
            registry = Registry.CurrentUser.CreateSubKey(
                "Software\\Microsoft\\Windows\\CurrentVersion\\Run"); //则创建指定的子项
        if (registry.GetValue("Kitopia") is null)
        {
            MessageBox msg = new MessageBox();
            msg.Title = "Kitopia";
            msg.Content = "是否设置开机自启?\n可能被杀毒软件阻止      ";
            msg.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            msg.CloseButtonText = "取消";
            msg.PrimaryButtonText = "确定";
            msg.FontSize = 15;
            var task = msg.ShowDialogAsync();
            // 使用ContinueWith来在任务完成后执行一个回调函数
            task.ContinueWith(e =>
            {
                MessageBoxResult result = e.Result;
                switch (result)
                {
                    case MessageBoxResult.Primary:
                    {
                        log.Info("用户确认启用开机自启");
                        try
                        {
                            registry.SetValue("Kitopia", $"\"{strName}\""); //设置该子项的新的“键值对”
                            new ToastContentBuilder()
                                .AddText("开机自启设置成功")
                                .Show();
                        }
                        catch (Exception exception)
                        {
                            log.Error("开机自启设置失败");
                            log.Error(exception.StackTrace);
                            new ToastContentBuilder()
                                .AddText("开机自启设置失败,请检查杀毒软件后重试")
                                .Show();
                        }

                        break;
                    }
                    case MessageBoxResult.None:
                    {
                        log.Info("用户取消启用开机自启");
                        break;
                    }
                }
            });
        }
        else if (!registry.GetValue("Kitopia").Equals($"\"{strName}\""))
        {
            try
            {
                registry.SetValue("Kitopia", $"\"{strName}\""); //设置该子项的新的“键值对”
                new ToastContentBuilder()
                    .AddText("开机自启设置成功")
                    .Show();
            }
            catch (Exception exception)
            {
                log.Error("开机自启设置失败");
                log.Error(exception.StackTrace);
                new ToastContentBuilder()
                    .AddText("开机自启设置失败,请检查杀毒软件后重试")
                    .Show();
            }
        }
        else
            log.Debug("程序自启已存在");
    }

    /// <summary>
    ///     Configures the services for the application.
    /// </summary>
    private static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IconTools>();
        services.AddTransient<IThemeChange, ThemeChange>();
        services.AddTransient<IToastService, ToastService>();
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddTransient<INavigationPageService, NavigationPageService>();
        services.AddTransient<IClipboardService, ClipboardService>();
#if DEBUG
        services.AddTransient<ITaskEditorOpenService, TaskEditorOpenService>();
#endif
        services.AddSingleton<SearchWindowViewModel>(e =>
        {
            return new SearchWindowViewModel { IsActive = true };
        });
        services.AddSingleton<SearchWindow>(sq =>
        {
            return new SearchWindow { DataContext = sq.GetService<SearchWindowViewModel>() };
        });
        services.AddSingleton<MainWindowViewModel>(e =>
        {
            return new MainWindowViewModel { IsActive = true };
        });
        services.AddSingleton<MainWindow>(sq =>
        {
            return new MainWindow { DataContext = sq.GetService<MainWindowViewModel>() };
        });
        services.AddTransient<SettingPageViewModel>(e =>
        {
            return new SettingPageViewModel { IsActive = true };
        });
        services.AddTransient<SettingPage>(e =>
        {
            return new SettingPage() { DataContext = e.GetService<SettingPageViewModel>() };
        });
        services.AddTransient<HomePageViewModel>(e =>
        {
            return new HomePageViewModel { IsActive = true };
        });
        services.AddTransient<HomePage>(e =>
        {
            return new HomePage() { DataContext = e.GetService<HomePageViewModel>() };
        });
        services.AddTransient<PluginManagerPageViewModel>(e =>
        {
            return new PluginManagerPageViewModel { IsActive = true };
        });
        services.AddTransient<PluginManagerPage>(e =>
        {
            return new PluginManagerPage() { DataContext = e.GetService<PluginManagerPageViewModel>() };
        });
        services.AddSingleton<PluginSettingViewModel>(e =>
        {
            return new PluginSettingViewModel { IsActive = true };
        });
        services.AddSingleton<PluginSetting>(e =>
        {
            return new PluginSetting() { DataContext = e.GetService<PluginSettingViewModel>() };
        });
        services.AddTransient<TaskEditorViewModel>(e =>
        {
            return new TaskEditorViewModel { IsActive = true };
        });
        services.AddTransient<TaskEditor>(e =>
        {
            return new TaskEditor() { DataContext = e.GetService<TaskEditorViewModel>() };
        });
        return services.BuildServiceProvider();
    }

    private void Current_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        e.Handled = true;
        Exprint(e);
    }

    private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        // Current.Dispatcher.BeginInvoke(new Action(delegate
        // {

        log.Error(e.ExceptionObject);
        var error = new ErrorDialog("", "（1）发生了一个错误！" + Environment.NewLine
                                                      + e.ExceptionObject);
        error.ShowDialog();
        // }));
    }

    private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        e.Handled = true;
        Exprint(e);
    }

    private void Exprint(DispatcherUnhandledExceptionEventArgs e)
    {
        Current.Dispatcher.BeginInvoke(new Action(delegate
        {
            try
            {
                e.Handled = true;
                if (e.Exception.InnerException == null)
                {
                    log.Error(e.Exception.StackTrace);
                    var error = new ErrorDialog("", "（1）发生了一个错误！" + e.Exception.Message + Environment.NewLine
                                                    + "（2）错误源：" + e.Exception.Source +
                                                    Environment.NewLine
                                                    + "（3）详细信息：" + e.Exception.StackTrace +
                                                    Environment.NewLine);
                    error.ShowDialog();

                    //MessageBox.Show("（1）发生了一个错误！请联系腐竹！" + Environment.NewLine
                    //                   + "（2）错误源：" + e.Exception.Source + Environment.NewLine
                    //                   + "（3）详细信息：" + e.Exception.Message + Environment.NewLine);
                    //+ "（4）报错区域：" + e.Exception.StackTrace);
                }
                else
                {
                    log.Error("（1）发生了一个错误！" + e.Exception.Message + Environment.NewLine
                              + "（2）错误源：" + e.Exception.InnerException.Source +
                              Environment.NewLine
                              + "（3）错误信息：" + e.Exception.Message + Environment.NewLine
                              + "（4）详细信息：" + e.Exception.InnerException.Message +
                              Environment.NewLine
                              + "（5）报错区域：" + e.Exception.InnerException.StackTrace);
                    var error = new ErrorDialog("", "（1）发生了一个错误！" + Environment.NewLine
                                                                  + "（2）错误源：" +
                                                                  e.Exception.InnerException.Source +
                                                                  Environment.NewLine
                                                                  + "（3）错误信息：" + e.Exception.Message +
                                                                  Environment.NewLine
                                                                  + "（4）详细信息：" +
                                                                  e.Exception.InnerException.Message +
                                                                  Environment.NewLine
                                                                  + "（5）报错区域：" + e.Exception.InnerException
                                                                      .StackTrace);
                    error.ShowDialog();
                    //MessageBox.Show("（1）发生了一个错误！" + Environment.NewLine
                    //                    + "（2）错误源：" + e.Exception.InnerException.Source + Environment.NewLine
                    //                    + "（3）错误信息：" + e.Exception.Message + Environment.NewLine
                    //                    + "（4）详细信息：" + e.Exception.InnerException.Message + Environment.NewLine
                    //                    + "（5）报错区域：" + e.Exception.InnerException.StackTrace);
                }
            }
            catch (Exception e2)
            {
                //此时程序出现严重异常，将强制结束退出
                log.Error(e2.StackTrace);
                System.Windows.MessageBox.Show("程序发生致命错误，将终止，");
            }
        })).Wait();
    }
}