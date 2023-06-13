using System.Windows;
using System.Windows.Controls;
using Core.SDKs.Services;
using Kitopia.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace Kitopia.View.Pages;

public partial class SettingPage : Page
{
    public SettingPage()
    {
        InitializeComponent();
    }

    private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
    {
        HotKeyEditorWindow hotKeyEditor = new HotKeyEditorWindow(((HotKeyShow)sender).Tag.ToString());
        hotKeyEditor.Height = ServiceManager.Services.GetService<MainWindow>().Height / 2;
        hotKeyEditor.Width = ServiceManager.Services.GetService<MainWindow>().Width / 2;
        hotKeyEditor.Owner = ServiceManager.Services.GetService<MainWindow>();
        hotKeyEditor.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        hotKeyEditor.Title = "修改快捷键";
        hotKeyEditor.ShowDialog();
    }
}