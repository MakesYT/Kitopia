#region

using System.Collections.Generic;
using System.Windows;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Metadata;
using Core.SDKs.CustomScenario;
using DataTemplate = Avalonia.Markup.Xaml.Templates.DataTemplate;

#endregion

namespace Kitopia.View.Pages.Plugin;

public class MyDataTemplateSelector : IDataTemplate
{
    // Override the SelectTemplate method
    [Content]
    public Dictionary<string, IDataTemplate> Templates {get;} = new Dictionary<string, IDataTemplate>();


    public Control? Build(object? item)
    {
        if (item is ConnectorItem pointItem)
        {
            
        
            // Check the type of the item and return the corresponding data template from the resources
            if (!pointItem.IsSelf)
            {
            
                return Templates["InputTemplate"].Build(item);
            }

            return (pointItem.RealType.FullName! switch
            {
                "System.String" => Templates["StringTemplate"].Build(item),
            
                "System.Int32" => Templates["IntTemplate"].Build(item),
            
                "System.Double" => Templates["DoubleTemplate"].Build(item),
                "System.Boolean" => Templates["BoolTemplate"].Build(item),
                "PluginCore.SearchViewItem" => Templates["SearchItemTemplate"].Build(item),
                _ => Templates["InputTemplate"].Build(item)
            })!;
        }

        return null;
    }

    public bool Match(object? data) => true;
}