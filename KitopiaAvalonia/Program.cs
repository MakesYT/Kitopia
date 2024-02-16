using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Fonts.Inter;
using Avalonia.Media;
using Avalonia.Media.Fonts;
using Avalonia.Threading;
using Core.Linux;
using Core.SDKs.Everything;
using Core.SDKs.Services;
using Core.SDKs.Tools;
using Core.ViewModel;
using Core.ViewModel.Pages;
using Core.ViewModel.Pages.customScenario;
using Core.ViewModel.Pages.plugin;
using Core.ViewModel.TaskEditor;
using Core.Window;
using DesktopNotifications;
using DesktopNotifications.Avalonia;
using Kitopia.Services;
using KitopiaAvalonia.Pages;
using KitopiaAvalonia.Services;
using KitopiaAvalonia.Windows;
using log4net;
using log4net.Config;
using Microsoft.Extensions.DependencyInjection;
using PluginCore;
using ReactiveUI;

namespace KitopiaAvalonia;

class Program
{
    private static readonly ILog Log = LogManager.GetLogger(nameof(Program));

    public static INotificationManager NotificationManager = null!;

    public static CancellationTokenSource cts = new CancellationTokenSource();

    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var logConfigStream = assembly.GetManifestResourceStream("KitopiaAvalonia.log4net.config")!;

        XmlConfigurator.Configure(logConfigStream);
        try
        {
            AppDomain.CurrentDomain.ProcessExit += (sender, eventArgs) =>
            {
                Program.cts.Cancel();
                Program.cts.Dispose();
            };

            RxApp.DefaultExceptionHandler = new MyCoolObservableExceptionHandler();
            TaskScheduler.UnobservedTaskException += (sender, eventArgs) =>
            {
                Log.Error(eventArgs.Exception);
                Dispatcher.UIThread.Invoke(() =>
                {
                    new ErrorDialog(null, eventArgs.Exception.ToString()).Show();
                });
                cts.Cancel();
                cts.Dispose();
            };
            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);
        }
        catch (Exception e)
        {
            Log.Fatal(e);
        }
        finally
        {
            cts.Cancel();
            cts.Dispose();
        }
    }

    static void AppMain(Application app, string[] args)
    {
        app.Run(cts.Token);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
    {
        var buildAvaloniaApp = AppBuilder.Configure<App>();
        buildAvaloniaApp.UsePlatformDetect();
        buildAvaloniaApp.With(new FontManagerOptions()
        {
            DefaultFamilyName = "avares://KitopiaAvalonia/Assets/HarmonyOS_Sans_SC_Regular.ttf#HarmonyOS Sans",
            FontFallbacks = new[]
            {
                new FontFallback()
                {
                    FontFamily =
                        new FontFamily("avares://KitopiaAvalonia/Assets/HarmonyOS_Sans_SC_Regular.ttf#HarmonyOS Sans")
                }
            },
        });
        buildAvaloniaApp.LogToTrace();
        buildAvaloniaApp.SetupDesktopNotifications(out NotificationManager!);
        return buildAvaloniaApp;
    }
}