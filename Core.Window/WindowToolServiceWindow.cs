﻿using System.Runtime.InteropServices;
using Avalonia;
using Core.SDKs.Services;
using Vanara.PInvoke;

namespace Core.Window;

public class WindowToolServiceWindow : IWindowTool
{

    public void SetForegroundWindow(IntPtr hWnd)
    {
        User32.SetForegroundWindow(hWnd);
    }

    public void MoveWindowToMouseScreenCenter(Avalonia.Controls.Window window)
    {
        User32.GetCursorPos(out var pos);
        var hmonitor = User32.MonitorFromPoint(pos, User32.MonitorFlags.MONITOR_DEFAULTTOPRIMARY);
        var monitorInfo = new User32.MONITORINFO();
        monitorInfo.cbSize = 40;
        User32.GetMonitorInfo(hmonitor, ref monitorInfo);
        
       window.Position = new PixelPoint((monitorInfo.rcMonitor.Left+(int)((monitorInfo.rcMonitor.Width-window.Bounds.Width  ) / 2)), monitorInfo.rcMonitor.Top+monitorInfo.rcMonitor.Height / 4);
    }
}