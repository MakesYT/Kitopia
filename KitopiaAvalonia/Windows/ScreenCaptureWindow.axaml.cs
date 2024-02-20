using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.Messaging;

namespace KitopiaAvalonia.Windows;

public partial class ScreenCaptureWindow : Window
{
    public ScreenCaptureWindow()
    {
        InitializeComponent();
        WeakReferenceMessenger.Default.Register<string, string>(this, "ScreenCapture", (sender, message) =>
        {
            if (Image.Source is Bitmap bitmap)
            {
                bitmap.Dispose();
            }

            Image.Source = null;
            this.Close();
            WeakReferenceMessenger.Default.Unregister<string>(this);
        });
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.Key == Key.Escape)
        {
            //KitopiaAvalonia.Tools.ScreenCapture.Dispose();
            WeakReferenceMessenger.Default.Send<string, string>("ScreenCapture", "ScreenCapture");
        }

        if (e.Key == Key.B)
        {
            WindowState = WindowState.Maximized;
        }

        if (e.Key == Key.C)
        {
            WindowState = WindowState.Normal;
        }
    }
}