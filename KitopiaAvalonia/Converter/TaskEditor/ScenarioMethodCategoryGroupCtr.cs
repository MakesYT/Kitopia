using System;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Core.SDKs.CustomScenario;

namespace Kitopia.Converter.TaskEditor;

public class ScenarioMethodCategoryGroupCtr : IValueConverter
{
    public IDataTemplate DataTemplate;

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (parameter is CompiledBindingExtension compiledBindingExtension)
        {
            if (compiledBindingExtension.DefaultAnchor.Target is Control control)
            {
                control.TryGetResource("DataTemplate", null, out var dataTemplate);
                DataTemplate = dataTemplate as IDataTemplate;
            }
        }

        Expander expander = new Expander();

        ItemsControl itemsControl = new ItemsControl();

        itemsControl.DataTemplates.Add(DataTemplate);

        //itemsControl.ItemTemplate=itemsControl.GetR
        if (value is ScenarioMethodCategoryGroup group)
        {
            expander.Header = "节点";


            expander.Content = itemsControl;
            Prase(group, itemsControl);
        }

        return expander;
    }

    private void Prase(ScenarioMethodCategoryGroup group, ItemsControl itemsControl)
    {
        foreach (var (key, scenarioMethodCategoryGroup) in group.Childrens)
        {
            var expander = new Expander();
            itemsControl.Items.Add(expander);
            itemsControl.DataTemplates.Add(DataTemplate);
            expander.Header = key;
            var control = new ItemsControl();
            expander.Content = control;
            Prase(scenarioMethodCategoryGroup, control);
        }

        foreach (var (key, value) in group.Methods)
        {
            itemsControl.Items.Add(value);
        }
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}