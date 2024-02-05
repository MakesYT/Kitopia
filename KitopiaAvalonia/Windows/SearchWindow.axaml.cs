using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Messaging;
using Core.SDKs.Services;
using Core.ViewModel;
using Microsoft.Extensions.DependencyInjection;
using PluginCore;
using Vanara.PInvoke;

namespace KitopiaAvalonia.Windows;

public partial class SearchWindow : Window
{
    public SearchWindow()
    {
        InitializeComponent();
        RenderOptions.SetTextRenderingMode(this, TextRenderingMode.Antialias);
        WeakReferenceMessenger.Default.Register<string, string>(this, "SearchWindowClose", (_, _) =>
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                IsVisible = false;
            });
        });
    }

    private void w_Deactivated(object? sender, EventArgs eventArgs)
    {
        IsVisible = false;
    }


    private void w_Activated(object sender, EventArgs e)
    {
        var hwnd = TryGetPlatformHandle().Handle;
        User32.SetForegroundWindow(hwnd);
        User32.SetFocus(hwnd);
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

    private void tx_KeyDown(object? sender, KeyEventArgs e)
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