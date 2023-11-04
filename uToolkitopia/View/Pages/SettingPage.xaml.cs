#region

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using log4net;

#endregion

namespace Kitopia.View.Pages;

public partial class SettingPage : Page
{
    private static readonly ILog log = LogManager.GetLogger(nameof(SettingPage));

    public SettingPage()
    {
        InitializeComponent();
    }


    private void ListBox_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        var dataGrid = (DataGrid)sender;
        var dependencyObject = GetScrollViewer(dataGrid);

        var bounds = dataGrid.TransformToAncestor(Window.GetWindow(this))
            .TransformBounds(new Rect(0.0, 0.0, dataGrid.ActualWidth, dataGrid.ActualHeight));
        var rect = new Rect(0.0, 0.0, WindowWidth, WindowHeight);

        {
            if (!rect.Contains(bounds))
            {
                e.Handled = true;
                var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta);
                eventArg.RoutedEvent = MouseWheelEvent;
                eventArg.Source = sender;
                var parent = ((Control)sender).Parent as UIElement;
                parent.RaiseEvent(eventArg);
            }
        }

        if (dependencyObject.ContentVerticalOffset >= dependencyObject.ScrollableHeight && e.Delta < 0)
        {
            var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta);
            eventArg.RoutedEvent = MouseWheelEvent;
            eventArg.Source = sender;
            var parent = ((Control)sender).Parent as UIElement;
            parent.RaiseEvent(eventArg);
        }

        if (dependencyObject.ContentVerticalOffset == 0 && e.Delta > 0)
        {
            var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta);
            eventArg.RoutedEvent = MouseWheelEvent;
            eventArg.Source = sender;
            var parent = ((Control)sender).Parent as UIElement;
            parent.RaiseEvent(eventArg);
        }
    }

    private ScrollViewer GetScrollViewer(DataGrid dataGrid)
    {
        if (VisualTreeHelper.GetChildrenCount(dataGrid) == 0)
        {
            return null;
        }

        // 获取DataGrid的第一个子元素，它是一个Border
        var border = VisualTreeHelper.GetChild(dataGrid, 0) as Border;

        // 获取Border的子元素，它是一个ScrollViewer
        var scrollViewer = border.Child as ScrollViewer;

        return scrollViewer;
    }
}