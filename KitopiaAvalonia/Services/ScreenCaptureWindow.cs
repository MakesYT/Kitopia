using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media;
using Core.SDKs.Services;
using Microsoft.Extensions.DependencyInjection;
using PluginCore;

namespace KitopiaAvalonia.Services;

public class ScreenCaptureWindow : IScreenCaptureWindow
{
    public void CaptureScreen()
    {
        var captureAllScreen = ServiceManager.Services.GetService<IScreenCapture>()!.CaptureAllScreen();
        if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var screens = desktop.MainWindow.Screens;
            for (var index = 0; index < screens.All.Count; index++)
            {
                var screen = screens.All[index];
                var rect = screen.Bounds;
                new Windows.ScreenCaptureWindow(index)
                {
                    Position = new PixelPoint(rect.X, rect.Y),
                    WindowState = WindowState.FullScreen,
                    SystemDecorations = SystemDecorations.None,
                    Background = new SolidColorBrush(Colors.Black),
                    ShowInTaskbar = false,
                    Topmost = true,
                    CanResize = false,
                    IsVisible = true,
                    MosaicImage =
                    {
                        Source = captureAllScreen.Item2.Count > 0 ? captureAllScreen.Item2.Dequeue() : null
                    },
                    Image =
                    {
                        Source = captureAllScreen.Item1.Count > 0 ? captureAllScreen.Item1.Dequeue() : null
                    }
                }.Show();
            }
        }

        captureAllScreen.Item2.Clear();
        captureAllScreen.Item1.Clear();
        GC.Collect(2, GCCollectionMode.Aggressive);
    }

    public async Task<ScreenCaptureInfo> GetScreenCaptureInfo()
    {
        ScreenCaptureInfo screenCaptureInfo = new ScreenCaptureInfo()
        {
            Index = -1
        };
        SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);
        Action<ScreenCaptureInfo> action = (info) =>
        {
            screenCaptureInfo = info;
            semaphore.Release();
        };
        await semaphore.WaitAsync();
        var captureAllScreen = ServiceManager.Services.GetService<IScreenCapture>()!.CaptureAllScreen();
        if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var screens = desktop.MainWindow.Screens;
            for (var index = 0; index < screens.All.Count; index++)
            {
                var screen = screens.All[index];
                var rect = screen.Bounds;
                var window = new Windows.ScreenCaptureWindow(index)
                {
                    Position = new PixelPoint(rect.X, rect.Y),
                    WindowState = WindowState.FullScreen,
                    SystemDecorations = SystemDecorations.None,
                    Background = new SolidColorBrush(Colors.Black),
                    ShowInTaskbar = false,
                    Topmost = false,
                    CanResize = false,
                    IsVisible = true,
                };

                window.MosaicImage.Source = captureAllScreen.Item2.Count > 0 ? captureAllScreen.Item2.Dequeue() : null;
                window.Image.Source = captureAllScreen.Item1.Count > 0 ? captureAllScreen.Item1.Dequeue() : null;
                window.SetToSelectMode(action);
                window.Show();
            }
        }

        await semaphore.WaitAsync();
        captureAllScreen.Item2.Clear();
        captureAllScreen.Item1.Clear();
        semaphore.Dispose();
        GC.Collect(2, GCCollectionMode.Aggressive);

        return screenCaptureInfo;
    }
}