using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
#if LINUX
    using Core.Linux;        
#endif

#if WINDOWS
using Core.Window;
using Core.Window.Everything;
#endif
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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using PluginCore;
using HotKeyManager = Core.SDKs.HotKey.HotKeyManager;

namespace KitopiaAvalonia;

public partial class App : Application
{
    private static readonly ILog log = LogManager.GetLogger("App");

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }


    public override void OnFrameworkInitializationCompleted()
    {
        ServiceManager.Services = ConfigureServices();
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = ServiceManager.Services.GetService<MainWindow>();
            DataContext = new AppViewModel();
            
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IToastService, ToastService>();
        services.AddTransient<IContentDialog, ContentDialogService>();
        services.AddTransient<IHotKeyEditor, HotKeyEditorService>();
        services.AddSingleton<ITaskEditorOpenService, TaskEditorOpenService>();

        services.AddTransient<IThemeChange, ThemeChange>();

        services.AddSingleton<ISearchItemChooseService, SearchItemChooseService>();
        services.AddSingleton<IMouseQuickWindowService, MouseQuickWindowService>();
        services.AddTransient<ISearchWindowService, SearchWindowService>();
        services.AddTransient<IErrorWindow, ErrorWindow>();
        services.AddTransient<IScreenCapture, Services.ScreenCapture>();
        
        services.AddTransient<INavigationPageService, NavigationPageService>();
        #if WINDOWS
        services.AddTransient<IEverythingService, EverythingService>();
        services.AddTransient<IAppToolService, AppToolService>();
        services.AddSingleton<ISearchItemTool, SearchItemTool>();
        services.AddTransient<IClipboardService, ClipboardWindow>();
        services.AddTransient<IWindowTool, WindowToolServiceWindow>();
        #endif

        #if LINUX
            services.AddTransient<IAppToolService, AppToolLinuxService>();
        #endif
        

        services.AddTransient<TaskEditorViewModel>();
        services.AddTransient<TaskEditor>(e => new TaskEditor() { DataContext = e.GetService<TaskEditorViewModel>() });
        services.AddSingleton<MainWindowViewModel>();
        services.AddSingleton<MainWindow>(e => new MainWindow() { DataContext = e.GetService<MainWindowViewModel>() });
        services.AddSingleton<SearchWindowViewModel>(e => new SearchWindowViewModel() { IsActive = true });
        services.AddSingleton<SearchWindow>(e => new SearchWindow()
            { DataContext = e.GetService<SearchWindowViewModel>() });
        services.AddTransient<MouseQuickWindowViewModel>();
        services.AddTransient<MouseQuickWindow>(e => new MouseQuickWindow()
            { DataContext = e.GetService<MouseQuickWindowViewModel>() });
        
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
        services.AddSingleton<PluginSettingViewModel>(e => new PluginSettingViewModel() { IsActive = true });
        services.AddKeyedSingleton<UserControl, PluginSettingSelectPage>("PluginSettingSelectPage",
            (e, _) => new PluginSettingSelectPage() { DataContext = e.GetService<PluginSettingViewModel>() });
        services.AddSingleton<SettingPage>(e => new SettingPage() );

        return services.BuildServiceProvider();
    }

    


   
}