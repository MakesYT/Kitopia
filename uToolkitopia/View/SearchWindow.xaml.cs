#region

using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using CommunityToolkit.Mvvm.Messaging;
using Core.SDKs.Services;
using Core.ViewModel;
using Microsoft.Extensions.DependencyInjection;
using PluginCore;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

#endregion

namespace Kitopia.View;

/// <summary>
///     SearchView.xaml 的交互逻辑
/// </summary>
public partial class SearchWindow : FluentWindow
{
    public SearchWindow()
    {
        InitializeComponent();

        WeakReferenceMessenger.Default.Register<string, string>(this, "SearchWindowClose", (_, _) =>
        {
            Application.Current.Dispatcher.BeginInvoke(() =>
            {
                Visibility = Visibility.Hidden;
            });
        });
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);
        // 获取窗体句柄
        var m_Hwnd = new WindowInteropHelper(this).Handle;
        ApplicationThemeManager.Changed += (theme, accent) =>
        {
            WindowBackdrop.ApplyBackdrop(m_Hwnd, WindowBackdropType.Acrylic);
        };
    }


    private void w_Deactivated(object sender, EventArgs e)
    {
        Visibility = Visibility.Hidden;
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr SetFocus(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    private void w_Activated(object sender, EventArgs e)
    {
        var hwnd = new WindowInteropHelper(this).Handle;
        SetForegroundWindow(hwnd);
        SetFocus(hwnd);
    }

    private void Button_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Down:
                break;
            case Key.Up:
                break;
            case Key.Enter:
                ((Button)sender).Command.Execute(((Button)sender).DataContext);
                break;
            default:
            {
                if (e.Key != Key.System)
                {
                    tx.Focus();
                }

                break;
            }
        }
    }

    private void tx_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            if (!dataGrid.Items.IsEmpty)
            {
                ServiceManager.Services!.GetService<SearchWindowViewModel>()!.OpenFile(
                    (SearchViewItem)dataGrid.Items[0]);
            }

            e.Handled = true;
        }
        else if (e.Key == Key.Down)
        {
            var keyArgs = new KeyEventArgs(Keyboard.PrimaryDevice, Keyboard.PrimaryDevice.ActiveSource, 0, Key.Tab);
            // 设置 RoutedEvent 属性为 Keyboard.KeyDownEvent
            keyArgs.RoutedEvent = Keyboard.KeyDownEvent;
            // 调用 InputManager.Current.ProcessInput 方法来处理该事件
            InputManager.Current.ProcessInput(keyArgs);
            e.Handled = true;
        }
    }
}