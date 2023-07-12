using System.Windows;
using System.Windows.Controls;

namespace Kitopia.View.Pages.Plugin;

public partial class PluginManagerPage : Page
{
    public PluginManagerPage()
    {
        InitializeComponent();
    }

    private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
    {
        new TaskEditor().Show();
    }
}