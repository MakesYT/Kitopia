using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using Core.SDKs;
using Core.SDKs.Config;
using Core.SDKs.Services;
using Core.SDKs.Tools;
using Core.ViewModel;
using Core.ViewModel.Pages;
using Kitopia.Services;
using Kitopia.View;
using log4net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Uwp.Notifications;
using Microsoft.Win32;
using Wpf.Ui.Controls.SnackbarControl;
using Wpf.Ui.Services;
using MessageBox = Wpf.Ui.Controls.MessageBoxControl.MessageBox;
using MessageBoxResult = Wpf.Ui.Controls.MessageBoxControl.MessageBoxResult;

namespace Kitopia;

/// <summary>
///     Interaction logic for App.xaml
/// </summary>
public sealed partial class App : Application
{
    private static readonly ILog log = LogManager.GetLogger("App");

    public App()
    {
    }

    [DllImport("user32.dll", EntryPoint = "SetForegroundWindow", SetLastError = true)]
    public static extern void SetForegroundWindow(IntPtr hwnd);

    protected override void OnStartup(StartupEventArgs e)
    {
        /*创建具有唯一名称的互斥锁*/
        var appMutex = new Mutex(true, "Kitopia", out var createdNew);

        if (!createdNew)
        {
            var current = Process.GetCurrentProcess();

            foreach (var process in Process.GetProcessesByName(current.ProcessName))
            {
                if (process.Id != current.Id)
                {
                    SetForegroundWindow(process.MainWindowHandle);
                    break;
                }
            }

            System.Windows.MessageBox.Show("不能同时开启两个应用", "Kitopia");
            Shutdown();
        }
        else
        {
#if !DEBUG
            log.Info("异常捕获");
            DispatcherUnhandledException += App_DispatcherUnhandledException;
            Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
#endif
            ServiceManager.Services = ConfigureServices();
            log.Info("Ioc初始化完成");
            ConfigManger.Init();
            log.Info("配置文件初始化完成");

            var initWindow = ServiceManager.Services.GetService<InitWindow>();
            initWindow.Show();
            Current.MainWindow = ServiceManager.Services.GetService<MainWindow>();
            ServicePointManager.DefaultConnectionLimit = 10240;

            if (ConfigManger.config.autoStart)
            {
                log.Info("设置开机自启");
                SetAutoStartup();
            }

            base.OnStartup(e);
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        ConfigManger.Save();
        base.OnExit(e);
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
        services.AddTransient<IClipboardService, ClipboardService>();
        services.AddSingleton<SearchWindowViewModel>(e =>
        {
            return new SearchWindowViewModel { IsActive = true };
        });
        services.AddSingleton<SearchWindow>(sq =>
        {
            return new SearchWindow { DataContext = sq.GetService<SearchWindowViewModel>() };
        });
        services.AddSingleton<InitWindowsViewModel>(e =>
        {
            return new InitWindowsViewModel { IsActive = true };
        });
        services.AddSingleton<InitWindow>(sq =>
        {
            return new InitWindow { DataContext = sq.GetService<InitWindowsViewModel>() };
        });
        services.AddSingleton<MainWindowViewModel>(e =>
        {
            return new MainWindowViewModel { IsActive = true };
        });
        services.AddSingleton<MainWindow>(sq =>
        {
            return new MainWindow { DataContext = sq.GetService<MainWindowViewModel>() };
        });
        services.AddSingleton<HomePageViewModel>();
        services.AddSingleton<SettingPageViewModel>(e =>
        {
            return new SettingPageViewModel { IsActive = true };
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
                    var error = new ErrorDialog("", "（1）发生了一个错误！" + Environment.NewLine
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
                    log.Error("（1）发生了一个错误！" + Environment.NewLine
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