#region

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Core.SDKs.Services;
using Kitopia.Controls;
using Microsoft.Extensions.DependencyInjection;

#endregion

namespace Kitopia.View.Pages;

public partial class SettingPage : Page
{
    public SettingPage()
    {
        InitializeComponent();
    }

    private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
    {
        var hotKeyEditor = new HotKeyEditorWindow(((HotKeyShow)sender).Tag.ToString());
        hotKeyEditor.Height = ServiceManager.Services.GetService<MainWindow>().Height / 2;
        hotKeyEditor.Width = ServiceManager.Services.GetService<MainWindow>().Width / 2;
        hotKeyEditor.Owner = ServiceManager.Services.GetService<MainWindow>();
        hotKeyEditor.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        hotKeyEditor.Title = "修改快捷键";
        hotKeyEditor.ShowDialog();
    }

    private void ListBox_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (!e.Handled)
        {
            // ListView拦截鼠标滚轮事件
            e.Handled = true;

            // 激发一个鼠标滚轮事件，冒泡给外层ListView接收到
            var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta);
            eventArg.RoutedEvent = UIElement.MouseWheelEvent;
            eventArg.Source = sender;
            var parent = ((Control)sender).Parent as UIElement;
            parent.RaiseEvent(eventArg);
        }
    }
}