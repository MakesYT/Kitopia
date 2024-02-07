using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Threading;
using KitopiaAvalonia.Services;
using KitopiaAvalonia.Windows;
using log4net;
using ReactiveUI;

namespace KitopiaAvalonia;

class Program
{
    private static readonly ILog Log = LogManager.GetLogger(nameof(Program));

    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        try
        {
            RxApp.DefaultExceptionHandler = new MyCoolObservableExceptionHandler();
            TaskScheduler.UnobservedTaskException += (sender, eventArgs) =>
            {
                Log.Error(eventArgs.Exception);
                new ErrorDialog(null, eventArgs.ToString()).Show();
            };
            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);
        }
        catch (Exception e)
        {
            Log.Fatal(e);

            Task.Run(() =>
            {
                Thread.CurrentThread.IsBackground = false;
                Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    var tcs = new TaskCompletionSource();
                    var dialog = new ErrorDialog(null, e.ToString());
                    dialog.Show();
                    dialog.Closed += (sender, args) =>
                    {
                        tcs.SetResult();
                    };
                    await tcs.Task;
                });
            }).Wait();
        }
    }


    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}