using System;
using System.Runtime.InteropServices;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Messaging;
using Core.SDKs.Services;
using Core.ViewModel;
using Microsoft.Extensions.DependencyInjection;
using PluginCore;
using Window = Avalonia.Controls.Window;

namespace KitopiaAvalonia.Windows;

public partial class SearchWindow : Window
{
    public SearchWindow()
    {
        InitializeComponent();
        WeakReferenceMessenger.Default.Register<string, string>(this, "SearchWindowClose", (_, _) =>
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                IsVisible = false;
            });
        });
    }

    private void w_Deactivated(object sender, EventArgs e)
    {
        IsVisible = false;
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr SetFocus(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    private void w_Activated(object sender, EventArgs e)
    {
        var hwnd = TryGetPlatformHandle().Handle;
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
            if (dataGrid.Items.Count != 0)
            {
                ServiceManager.Services!.GetService<SearchWindowViewModel>()!.OpenFile(
                    (SearchViewItem)dataGrid.Items[0]);
            }

            e.Handled = true;
        }
        else if (e.Key == Key.Down)
        {
            var keyArgs = new KeyEventArgs() { Key = Key.Tab };
            // 设置 RoutedEvent 属性为 Keyboard.KeyDownEvent
            keyArgs.RoutedEvent = KeyDownEvent;
            // 调用 InputManager.Current.ProcessInput 方法来处理该事件
            RaiseEvent(keyArgs);
            e.Handled = true;
        }
    }
}