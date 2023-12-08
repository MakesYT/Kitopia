using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using Core.ViewModel;
using log4net;
using PluginCore;
using Vanara.PInvoke;
using WindowsInput;
using WindowsInput.Native;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace Kitopia.View;

public partial class MouseQuickWindow
{
    private static readonly ILog log = LogManager.GetLogger(nameof(MouseQuickWindow));
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


        /*var currentThreadId = Kernel32.GetCurrentThreadId();
        var foregroundWindow = User32.GetForegroundWindow();
        var pid = User32.GetWindowThreadProcessId(foregroundWindow, out var pid1);
        User32.AttachThreadInput(pid,currentThreadId , true);
        var sb = new StringBuilder(256);
        var hWnd = User32.GetFocus();
        User32.SendMessage(hWnd, User32.WindowMessage.WM_GETTEXT, sb.Capacity, sb);
        User32.AttachThreadInput(pid,currentThreadId , false);

        log.Info(sb.ToString());*/
        // var data = Clipboard.GetDataObject();
        string? text = null;
        if (Clipboard.ContainsText())
        {
            text = Clipboard.GetText();
        }

        var keyboardSimulator = new InputSimulator().Keyboard;
        keyboardSimulator.ModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_C).Sleep(200);

        Application.Current.Dispatcher.BeginInvoke(() =>
        {
            Task.Delay(800);
            var s = Clipboard.GetText();
            if (s != text)
            {
                ((MouseQuickWindowViewModel)DataContext).SelectedItem = new SelectedItem()
                    { type = FileType.文本, obj = Clipboard.GetText() };
                //log.Info(Clipboard.GetText());
            }

            if (text != null)
            {
                Clipboard.SetText(text);
            }
        });


        User32.SetForegroundWindow(m_Hwnd);
    }
}