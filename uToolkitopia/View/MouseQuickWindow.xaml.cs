using System;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using Vanara.PInvoke;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace Kitopia.View;

public partial class MouseQuickWindow
{
    IntPtr m_Hwnd = IntPtr.Zero;

    public MouseQuickWindow()
    {
        InitializeComponent();
    }

    private void MouseQuickWindow_OnDeactivated(object? sender, EventArgs e)
    {
        if (IsVisible)
        {
            this.Close();
        }
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);
        // 获取窗体句柄
        m_Hwnd = new WindowInteropHelper(this).Handle;
        ApplicationThemeManager.Changed += (theme, accent) =>
        {
            WindowBackdrop.ApplyBackdrop(m_Hwnd, WindowBackdropType.Acrylic);
        };
        User32.GetCursorPos(out var pos);
        var hmonitor = User32.MonitorFromPoint(pos, User32.MonitorFlags.MONITOR_DEFAULTTOPRIMARY);
        var monitorInfo = new User32.MONITORINFO();
        monitorInfo.cbSize = 40;
        User32.GetMonitorInfo(hmonitor, ref monitorInfo);
        var windowinfo = new User32.WINDOWINFO();
        windowinfo.cbSize = (uint)Marshal.SizeOf(windowinfo);
        User32.GetWindowInfo(m_Hwnd, ref windowinfo);
        if (monitorInfo.rcMonitor.Width < pos.X + windowinfo.rcClient.Width)
        {
            this.Left = pos.X - windowinfo.rcClient.Width;
        }
        else
        {
            this.Left = pos.X;
        }

        if (monitorInfo.rcMonitor.Height < pos.Y + windowinfo.rcClient.Height)
        {
            this.Top = pos.Y - windowinfo.rcClient.Height;
        }
        else
        {
            this.Top = pos.Y;
        }

        User32.SetForegroundWindow(m_Hwnd);
    }
}