#region

using System.Windows;
using System.Windows.Controls;
using Core.SDKs.Services;

#endregion

namespace Kitopia.View.Pages.Plugin;

public partial class PluginManagerPage : Page
{
    public PluginManagerPage()
    {
        InitializeComponent();
    }

    private void ButtonBase_OnClick(object sender, RoutedEventArgs e) =>
        ((TaskEditor)ServiceManager.Services!.GetService(typeof(TaskEditor))!).Show();
}