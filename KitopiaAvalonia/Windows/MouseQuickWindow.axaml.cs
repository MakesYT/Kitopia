using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Core.ViewModel;
using PluginCore;
using Vanara.PInvoke;
using WindowsInput;

namespace KitopiaAvalonia.Windows;

public partial class MouseQuickWindow : Window
{
    public MouseQuickWindow()
    {
        InitializeComponent();
    }

    private void WindowBase_OnDeactivated(object? sender, EventArgs e)
    {
        if (IsVisible)
        {
            this.Close();
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        User32.GetCursorPos(out var pos);
        var hmonitor = User32.MonitorFromPoint(pos, User32.MonitorFlags.MONITOR_DEFAULTTOPRIMARY);
        var monitorInfo = new User32.MONITORINFO();
        monitorInfo.cbSize = 40;
        User32.GetMonitorInfo(hmonitor, ref monitorInfo);
        var windowinfo = new User32.WINDOWINFO();
        windowinfo.cbSize = (uint)Marshal.SizeOf(windowinfo);
        User32.GetWindowInfo(TryGetPlatformHandle().Handle, ref windowinfo);

        int Left, Top;
        if (monitorInfo.rcMonitor.Width < pos.X + windowinfo.rcClient.Width)
        {
            Left = pos.X - windowinfo.rcClient.Width;
        }
        else
        {
            Left = pos.X;
        }

        if (monitorInfo.rcMonitor.Height < pos.Y + windowinfo.rcClient.Height)
        {
            Top = pos.Y - windowinfo.rcClient.Height;
        }
        else
        {
            Top = pos.Y;
        }

        Position = new PixelPoint(Left, Top);

        string? text = null;
        if (Clipboard.GetFormatsAsync().Result.Contains("Text"))
        {
            text = Clipboard.GetTextAsync().Result;
        }

        var keyboardSimulator = new InputSimulator().Keyboard;
        keyboardSimulator.ModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_C).Sleep(200);

        Dispatcher.UIThread.InvokeAsync(() =>
        {
            Task.Delay(800);
            var s = Clipboard.GetTextAsync().Result;
            if (s != text)
            {
                ((MouseQuickWindowViewModel)DataContext).SelectedItem = new SelectedItem()
                    { type = FileType.文本, obj = s };
                //log.Info(Clipboard.GetText());
            }

            if (text != null)
            {
                Clipboard.SetTextAsync(text);
            }
        });


        User32.SetForegroundWindow(TryGetPlatformHandle().Handle);
    }
}