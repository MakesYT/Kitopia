using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Threading;
using Core.SDKs;
using Core.SDKs.Config;
using Core.SDKs.Services;
using Core.ViewModel;
using IWshRuntimeLibrary;
using Kitopia.View;
using log4net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;

namespace Kitopia;

/// <summary>
///     Interaction logic for App.xaml
/// </summary>
public sealed partial class App : Application
{
    private static readonly ILog log = LogManager.GetLogger("App");

    public App()
    {
        ServiceManager.Services = ConfigureServices();
        ConfigManger.Init();
    }
    
    protected override void OnStartup(StartupEventArgs e)
    {
        DispatcherUnhandledException += App_DispatcherUnhandledException;
        Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

        SetAutoStartup();
        ServicePointManager.DefaultConnectionLimit = 10240;
        base.OnStartup(e);
    }

    protected override void OnExit(ExitEventArgs e)
    {
        ConfigManger.Save();
        base.OnExit(e);
    }

    private void SetAutoStartup()
    {
        
        
        string strName = AppDomain.CurrentDomain.BaseDirectory + "Kitopia.exe";//获取要自动运行的应用程序名
        if (!System.IO.File.Exists(strName))//判断要自动运行的应用程序文件是否存在
            return;
        RegistryKey registry = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run", true);//检索指定的子项
        if (registry == null)//若指定的子项不存在
            registry = Registry.CurrentUser.CreateSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run");//则创建指定的子项
        registry.SetValue("Kitopia", $"\"{strName}\"");//设置该子项的新的“键值对”
        

        
        
    }

    /// <summary>
    ///     Configures the services for the application.
    /// </summary>
    private static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();
        services.AddSingleton<SearchViewModel>();
        services.AddSingleton<SearchView>(sq =>
        {
           return new SearchView() { DataContext = sq.GetService<SearchViewModel>() };
        });
        services.AddSingleton<MainWindowsViewModel>();
        services.AddSingleton<MainWindows>(sq =>
        {
            return new MainWindows() { DataContext = sq.GetService<MainWindowsViewModel>() };
        });
        services.AddSingleton<GetIconFromFile>();
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
                    log.Error("（1）发生了一个错误！" + Environment.NewLine
                                            + "（2）错误源：" + e.Exception.Source + Environment.NewLine
                                            + "（3）详细信息：" + e.Exception.Message + Environment.NewLine);
                    var error = new ErrorDialog("", "（1）发生了一个错误！" + Environment.NewLine
                                                                  + "（2）错误源：" + e.Exception.Source +
                                                                  Environment.NewLine
                                                                  + "（3）详细信息：" + e.Exception.Message +
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
                
                MessageBox.Show("程序发生致命错误，将终止，");
            }
        })).Wait();
    }
}