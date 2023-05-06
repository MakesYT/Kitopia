﻿using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Threading;
using Core.SDKs.Config;
using Core.ViewModel;
using IWshRuntimeLibrary;
using Kitopia.View;
using log4net;
using Microsoft.Extensions.DependencyInjection;

namespace Kitopia;

/// <summary>
///     Interaction logic for App.xaml
/// </summary>
public sealed partial class App : Application
{
    private static readonly ILog log = LogManager.GetLogger("App");

    public App()
    {
        Services = ConfigureServices();
        ConfigManger.Init();
    }

    /// <summary>
    ///     Gets the current <see cref="App" /> instance in use
    /// </summary>
    public new static App Current => (App)Application.Current;

    /// <summary>
    ///     Gets the <see cref="IServiceProvider" /> instance to resolve application services.
    /// </summary>
    public IServiceProvider Services { get; }

    protected override void OnStartup(StartupEventArgs e)
    {
        DispatcherUnhandledException += App_DispatcherUnhandledException;
        Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

        setAutoStartup();
        ServicePointManager.DefaultConnectionLimit = 10240;
        base.OnStartup(e);
    }

    protected override void OnExit(ExitEventArgs e)
    {
        ConfigManger.Save();
        base.OnExit(e);
    }

    private void setAutoStartup()
    {
        var startupPath = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
        var shortcutPath = Path.Combine(startupPath, "Kitopia.lnk");
        var shell = new WshShell();
        var shortcut = (IWshShortcut)shell.CreateShortcut(shortcutPath);
        shortcut.TargetPath = Process.GetCurrentProcess().MainModule.FileName;
        shortcut.Save();
    }

    /// <summary>
    ///     Configures the services for the application.
    /// </summary>
    private static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();
        services.AddSingleton<SearchView>();
        services.AddSingleton<SearchViewModel>();
        services.AddSingleton<MainWindows>();
        services.AddSingleton<MainWindowsViewModel>();
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
                var error = new ErrorDialog("", "程序发生致命错误，将终止，");
                error.ShowDialog();
                //MessageBox.Show("程序发生致命错误，将终止，");
            }
        })).Wait();
    }
}