using System;
using System.Drawing;
using System.Formats.Asn1;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;
using Core.SDKs;
using Core.SDKs.Services;
using Core.ViewModel;
using Microsoft.Extensions.DependencyInjection;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;
using Wpf.Ui.Controls.Window;

namespace Kitopia.View;

/// <summary>
///     SearchView.xaml 的交互逻辑
/// </summary>
public partial class SearchWindow : FluentWindow
{
    public SearchWindow()
    {
        InitializeComponent();
        
    }
    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);
        // 获取窗体句柄
        IntPtr m_Hwnd = new WindowInteropHelper(this).Handle;
        Wpf.Ui.Appearance.Theme.Changed += ((theme, accent) =>
        {
            WindowBackdrop.ApplyBackdrop(m_Hwnd, WindowBackdropType.Acrylic);
        });

    }

    private void ChangeTheme(ThemeType currentTheme, Color systemAccent)
    {
        
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
        if (e.Key == Key.Down)
        {
            // 跳过 button 里面的控件，移动焦点到下一个单元格
            var keyArgs = new KeyEventArgs(Keyboard.PrimaryDevice, Keyboard.PrimaryDevice.ActiveSource, 0, Key.Down);
            // 设置 RoutedEvent 属性为 Keyboard.KeyDownEvent
            keyArgs.RoutedEvent = Keyboard.KeyDownEvent;
            // 调用 InputManager.Current.ProcessInput 方法来处理该事件
            InputManager.Current.ProcessInput(keyArgs);

            // 设置事件已处理，阻止默认行为
            //e.Handled = true;
        }
        else if (e.Key == Key.Up)
        {
            // 创建一个 KeyEventArgs 参数，表示 shift 按键的事件
            var shiftArgs = new KeyEventArgs(Keyboard.PrimaryDevice, Keyboard.PrimaryDevice.ActiveSource, 0,
                Key.LeftShift);
            // 设置 RoutedEvent 属性为 Keyboard.KeyDownEvent
            shiftArgs.RoutedEvent = Keyboard.KeyDownEvent;
            // 调用 InputManager.Current.ProcessInput 方法来处理该事件
            InputManager.Current.ProcessInput(shiftArgs);

            // 创建一个 KeyEventArgs 参数，表示 tab 按键的事件
            var tabArgs = new KeyEventArgs(Keyboard.PrimaryDevice, Keyboard.PrimaryDevice.ActiveSource, 0, Key.Tab);
            // 设置 RoutedEvent 属性为 Keyboard.KeyDownEvent
            tabArgs.RoutedEvent = Keyboard.KeyDownEvent;
            // 调用 InputManager.Current.ProcessInput 方法来处理该事件
            InputManager.Current.ProcessInput(tabArgs);

            // 创建一个 KeyEventArgs 参数，表示 shift 按键的事件
            var shiftUpArgs = new KeyEventArgs(Keyboard.PrimaryDevice, Keyboard.PrimaryDevice.ActiveSource, 0,
                Key.LeftShift);
            // 设置 RoutedEvent 属性为 Keyboard.KeyUpEvent
            shiftUpArgs.RoutedEvent = Keyboard.KeyUpEvent;
            // 调用 InputManager.Current.ProcessInput 方法来处理该事件
            InputManager.Current.ProcessInput(shiftUpArgs);
        }
    }

    private void tx_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            ServiceManager.Services!.GetService<SearchWindowViewModel>()!.OpenFile((SearchViewItem)dataGrid.Items[0]);
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