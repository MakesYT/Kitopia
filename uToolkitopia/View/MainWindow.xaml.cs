using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Interop;
using Core.SDKs.Services;
using Kitopia.SDKs;
using Microsoft.Extensions.DependencyInjection;
using Wpf.Ui.Controls.Navigation;
using Wpf.Ui.Controls.Window;

namespace Kitopia.View;

public partial class MainWindow : FluentWindow
{
    public MainWindow()
    {
        InitializeComponent();
        
    }
    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);
        // 获取窗体句柄
        IntPtr m_Hwnd = new WindowInteropHelper(this).Handle;
        var hWndSource = HwndSource.FromHwnd(m_Hwnd);
        ServiceManager.Services.GetService<InitWindow>().NotifyIcon.HookWindow = hWndSource;
        Wpf.Ui.Appearance.Theme.Changed += ((theme, accent) =>
        {
            WindowBackdrop.ApplyBackdrop(m_Hwnd, WindowBackdropType.Acrylic);
        });
        
    }

    private void MainWindow_OnClosing(object? sender, CancelEventArgs e)
    {
        e.Cancel = true;
        this.Visibility = Visibility.Hidden;
    }


    private void ThemeButton_OnClick(object sender, RoutedEventArgs e)
    {
        var currentTheme = Wpf.Ui.Appearance.Theme.GetAppTheme();

        Wpf.Ui.Appearance.Theme.Apply(currentTheme == Wpf.Ui.Appearance.ThemeType.Light ? Wpf.Ui.Appearance.ThemeType.Dark : Wpf.Ui.Appearance.ThemeType.Light);
        
    }
}