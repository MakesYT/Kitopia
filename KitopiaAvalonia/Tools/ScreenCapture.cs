using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media;
using Core.SDKs.Services;
using KitopiaAvalonia.Windows;
using Microsoft.Extensions.DependencyInjection;

namespace KitopiaAvalonia.Tools;

public class ScreenCapture
{
    public static void StartUserManualCapture()
    {
        var captureAllScreen = ServiceManager.Services.GetService<IScreenCapture>()!.CaptureAllScreen();
        if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var screens = desktop.MainWindow.Screens;
            foreach (var screen in screens.All)
            {
                var rect = screen.Bounds;
                var window = new ScreenCaptureWindow
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
                window.Show();
            }
        }

        captureAllScreen.Item2.Clear();
        captureAllScreen.Item1.Clear();
        GC.Collect(2, GCCollectionMode.Aggressive);
    }
}