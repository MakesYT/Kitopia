using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media;
using KitopiaAvalonia.Windows;

namespace KitopiaAvalonia.Tools;

public class ScreenCapture
{
    public static void StartUserManualCapture()
    {
        var captureAllScreen = Core.SDKs.Tools.ScreenCapture.CaptureAllScreen();
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
                window.Image.Source = captureAllScreen.Count > 0 ? captureAllScreen.Dequeue() : null;
                window.Show();
            }
        }

        captureAllScreen.Clear();
        GC.Collect(2, GCCollectionMode.Aggressive);
    }
}