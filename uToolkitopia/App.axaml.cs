#region

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Navigation;
using System.Windows.Threading;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.Messaging;
using Core.SDKs.Services;
using Core.SDKs.Services.Config;
using Core.SDKs.Tools;
using Core.ViewModel;
using Core.ViewModel.Pages;
using Core.ViewModel.Pages.customScenario;
using Core.ViewModel.Pages.plugin;
using Core.ViewModel.TaskEditor;
using Kitopia.Services;
using Kitopia.View;
using Kitopia.View.Pages;
using Kitopia.View.Pages.Plugin;
using log4net;
using Microsoft.Extensions.DependencyInjection;
using PluginCore;
using Vanara.PInvoke;
using Application = Avalonia.Application;
using ContentDialogService = Kitopia.Services.ContentDialogService;

#endregion

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

    public override void Initialize()
    {
        OnStartup();
        AvaloniaXamlLoader.Load(this);
        base.Initialize();
        DataContext = new MainViewModel();
    }


    private object? ShowHelper(Window window)
    {
        var type = typeof(Window);
        if ((bool)type.GetField("_disposed", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(window)!)
        {
            return null;
        }

        type.GetField("_isClosing", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(window, false);
        if ((bool)type.GetField("_isVisible", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(window)!)
        {
            return null;
        }

        typeof(Window).GetMethod("SetShowKeyboardCueState", BindingFlags.NonPublic | BindingFlags.Instance)!
            .Invoke(window, null);
        typeof(Window).GetMethod("SafeCreateWindowDuringShow", BindingFlags.NonPublic | BindingFlags.Instance)!
            .Invoke(window, null);
        User32.ShowWindow(new HandleRef(window,
            (IntPtr)type.GetProperty("CriticalHandle", BindingFlags.Instance | BindingFlags.NonPublic)!
                .GetValue(window)!).Handle, ShowWindowCommand.SW_HIDE);
        typeof(Window).GetMethod("SafeStyleSetter", BindingFlags.NonPublic | BindingFlags.Instance)!.Invoke(window,
            null);


        return null;
    }

    private static void Application_ApplicationExit(object? sender, EventArgs e)
    {
        ConfigManger.Save();
        WeakReferenceMessenger.Default.Send("Kitopia_SoftwareShutdown", "CustomScenarioTrigger");
        log.Info("程序退出");
    }


    /// <summary>
    ///     Configures the services for the application.
    /// </summary>
    private static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IconTools>();
        services.AddTransient<IThemeChange, ThemeChange>();
        services.AddSingleton<IToastService, ToastService>();
        services.AddTransient<ISearchItemTool, SearchItemTool>();
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddTransient<INavigationPageService, NavigationPageService>();
        services.AddTransient<ILabelWindowService, LabelWindowService>();
        services.AddTransient<IClipboardService, ClipboardService>();
        services.AddTransient<ITaskEditorOpenService, TaskEditorOpenService>();
        services.AddTransient<IContentDialog, ContentDialogService>();
        services.AddTransient<IHotKeyEditor, HotKeyEditorService>();
        services.AddTransient<ISearchItemChooseService, SearchItemChooseService>();
        services.AddSingleton<SearchWindowViewModel>(e => new SearchWindowViewModel { IsActive = true });
        services.AddSingleton<SearchWindow>(sq => new SearchWindow
            { DataContext = sq.GetService<SearchWindowViewModel>() });
        services.AddSingleton<MainWindowViewModel>(e => new MainWindowViewModel { IsActive = true });
        services.AddSingleton<MainWindow>(sq => new MainWindow { DataContext = sq.GetService<MainWindowViewModel>() });
        services.AddTransient<SettingPageViewModel>(e => new SettingPageViewModel { IsActive = true });
        services.AddTransient<SettingPage>(e =>
            new SettingPage { DataContext = e.GetService<SettingPageViewModel>() });
        services.AddTransient<HomePageViewModel>(e => new HomePageViewModel { IsActive = true });
        services.AddTransient<HomePage>(e => new HomePage { DataContext = e.GetService<HomePageViewModel>() });
        services.AddTransient<PluginManagerPageViewModel>(e => new PluginManagerPageViewModel { IsActive = true });
        services.AddTransient<PluginManagerPage>(e => new PluginManagerPage
            { DataContext = e.GetService<PluginManagerPageViewModel>() });
        services.AddSingleton<PluginSettingViewModel>(e => new PluginSettingViewModel { IsActive = true });
        services.AddSingleton<PluginSetting>(e => new PluginSetting
            { DataContext = e.GetService<PluginSettingViewModel>() });
        services.AddTransient<TaskEditorViewModel>(e => new TaskEditorViewModel { IsActive = true });
        services.AddTransient<TaskEditor>(e => new TaskEditor { DataContext = e.GetService<TaskEditorViewModel>() });
        services.AddTransient<CustomScenariosManagerPageViewModel>(e => new CustomScenariosManagerPageViewModel
            { IsActive = true });
        services.AddTransient<CustomScenariosManagerPage>(e => new CustomScenariosManagerPage
            { DataContext = e.GetService<CustomScenariosManagerPageViewModel>() });
        services.AddTransient<LabelWindowViewModel>(e => new LabelWindowViewModel());
        services.AddTransient<LabelWindow>(e => new LabelWindow()
            { DataContext = e.GetService<LabelWindowViewModel>() });

        services.AddTransient<HotKeyManagerPageViewModel>(e => new HotKeyManagerPageViewModel());
        services.AddTransient<HotKeyManagerPage>(e => new HotKeyManagerPage()
            { DataContext = e.GetService<HotKeyManagerPageViewModel>() });

        services.AddTransient<MouseQuickWindowViewModel>(e => new MouseQuickWindowViewModel());
        services.AddTransient<MouseQuickWindow>(e => new MouseQuickWindow()
            { DataContext = e.GetService<MouseQuickWindowViewModel>() });
        return services.BuildServiceProvider();
    }

    private void Current_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        e.Handled = true;
        Exprint(e);
    }

    private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        log.Error(e.ExceptionObject);
        var error = new ErrorDialog("", "（1）发生了一个错误！" + Environment.NewLine
                                                      + e.ExceptionObject);
        error.ShowDialog();
    }

    private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        e.Handled = true;
        Exprint(e);
    }

    private void Exprint(DispatcherUnhandledExceptionEventArgs e) =>
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