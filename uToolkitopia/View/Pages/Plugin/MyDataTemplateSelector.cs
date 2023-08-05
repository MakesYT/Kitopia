using System.Windows;
using System.Windows.Controls;
using Core.ViewModel.TaskEditor;

namespace Kitopia.View.Pages.Plugin;

public class MyDataTemplateSelector : DataTemplateSelector
{
    // Override the SelectTemplate method
    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    {
        // Get the current framework element
        var element = container as FrameworkElement;
        if (element != null && item != null)
        {
            // Check the type of the item and return the corresponding data template from the resources
            if (item is ConnectorItem pointItem && pointItem.IsSelf)
            {
                if (pointItem.Type.FullName == "System.String")
                {
                    return element.FindResource("StringTemplate") as DataTemplate;
                }
            }

            return element.FindResource("InputTemplate") as DataTemplate;
        }

        // Return the default data template if no match is found
        return base.SelectTemplate(item, container);
    }
}