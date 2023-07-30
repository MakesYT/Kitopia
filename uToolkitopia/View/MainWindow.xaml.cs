using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Interop;
using Core.SDKs.Services;
using Microsoft.Extensions.DependencyInjection;
using Wpf.Ui.Contracts;
using Wpf.Ui.Controls;

namespace Kitopia.View;

public partial class MainWindow
{
    public MainWindow()
    {
        InitializeComponent();
        NavigationView.SetServiceProvider(ServiceManager.Services!);

        ServiceManager.Services!.GetService<INavigationService>()!.SetNavigationControl(NavigationView);
        //TOD
#if DEBUG
        new TaskEditor().Show();
#endif
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
    }
}