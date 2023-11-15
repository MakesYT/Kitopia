#region

using System.Windows;
using System.Windows.Controls;
using Core.SDKs.CustomScenario;

#endregion

namespace Kitopia.View.Pages.Plugin;

public class MyDataTemplateSelector : DataTemplateSelector
{
    // Override the SelectTemplate method
    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    {
        // Get the current framework element
        var element = container as FrameworkElement;
        if (element == null || item == null)
        {
            return base.SelectTemplate(item, container);
        }

        // Check the type of the item and return the corresponding data template from the resources
        if (item is not ConnectorItem { IsSelf: true } pointItem)
        {
            return element.FindResource("InputTemplate") as DataTemplate;
        }

        return (pointItem.RealType.FullName! switch
        {
            "System.String" => element.FindResource("StringTemplate")! as DataTemplate,
            "System.Int32" => element.FindResource("IntTemplate")! as DataTemplate,
            "System.Double" => element.FindResource("DoubleTemplate")! as DataTemplate,
            "System.Boolean" => element.FindResource("BoolTemplate")! as DataTemplate,
            "PluginCore.SearchViewItem" => element.FindResource("SearchItemTemplate")! as DataTemplate,
            _ => element.FindResource("InputTemplate")! as DataTemplate
        })!;

        // Return the default data template if no match is found
    }
}