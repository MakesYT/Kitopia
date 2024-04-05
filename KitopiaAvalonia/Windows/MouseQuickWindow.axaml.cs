﻿using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Avalonia.Xaml.Interactivity;
using Core.ViewModel;
using Desktop.Robot;
using PluginCore;
using SharpHook;
using SharpHook.Native;
using Vanara.PInvoke;
using Key = Desktop.Robot.Key;

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
        if (pos.X + windowinfo.rcClient.Width<monitorInfo.rcMonitor.Right)
        {
            Left = pos.X;
        }
        else
        {
            Left = pos.X - windowinfo.rcClient.Width;
        }
        
       
        if (pos.Y + windowinfo.rcClient.Height < monitorInfo.rcMonitor.Bottom)
        {
            Top = pos.Y;
        }
        else
        {
            Top = pos.Y - windowinfo.rcClient.Height;
        }

        Position = new PixelPoint(Left, Top);

        string? text = null;
        if (Clipboard.GetFormatsAsync().Result.Contains("Text"))
        {
            text = Clipboard.GetTextAsync().Result;
        }


        var robot = new Robot();
        robot.AutoDelay = 1;
        robot.KeyDown(Key.Control);
        robot.KeyDown(Key.C);
        Task.Delay(200);
        robot.KeyUp(Key.C);
        robot.KeyUp(Key.Control);

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