using System;
using System.Reflection;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Media;
using Avalonia.Threading;
using DesktopNotifications;
using DesktopNotifications.Avalonia;
using KitopiaAvalonia.Services;
using KitopiaAvalonia.Windows;
using log4net;
using log4net.Config;
using ReactiveUI;

namespace KitopiaAvalonia;

class Program
{
    private static readonly ILog Log = LogManager.GetLogger(nameof(Program));

    public static INotificationManager NotificationManager = null!;


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
            RxApp.DefaultExceptionHandler = new MyCoolObservableExceptionHandler();
            TaskScheduler.UnobservedTaskException += (sender, eventArgs) =>
            {
                Log.Error(eventArgs.Exception);
                Dispatcher.UIThread.Invoke(() =>
                {
                    new ErrorDialog(null, eventArgs.Exception.ToString()).Show();
                });
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
        }
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