﻿#region

using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Metadata;
using Core.SDKs.CustomScenario;

#endregion

namespace KitopiaAvalonia.Pages;

public class MyDataTemplateSelector : IDataTemplate
{
    // Override the SelectTemplate method
    [Content] public Dictionary<string, IDataTemplate> Templates { get; } = new Dictionary<string, IDataTemplate>();


    public Control? Build(object? item)
    {
        if (item is ConnectorItem pointItem)
        {
            // Check the type of the item and return the corresponding data template from the resources
            if (!pointItem.IsSelf)
            {
                return Templates["InputTemplate"]
                    .Build(item);
            }

            if (pointItem.isPluginInputConnector)
            {
                var control = pointItem.PluginInputConnector.IDataTemplate.Build(item);
                pointItem.PluginInputConnector.Value.Subscribe(x => { pointItem.InputObject = x; });
                control!.Styles.Add(pointItem.PluginInputConnector.Style);
                return control;
            }

            if (pointItem.RealType.BaseType.FullName == "System.Enum")
            {
                StackPanel stackPanel = new StackPanel();
                stackPanel.Orientation = Orientation.Horizontal;
                stackPanel.VerticalAlignment = VerticalAlignment.Center;
                stackPanel.HorizontalAlignment = HorizontalAlignment.Right;
                TextBlock textBlock = new TextBlock
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    Name = "textBlock",
                    Foreground = Application.Current.Resources["TextFillColorPrimaryBrush"] as IBrush,
                };
                var binding = new Binding();
                binding.Path = "Title";
                textBlock.Bind(TextBlock.TextProperty, binding);
                stackPanel.Children.Add(textBlock);
                var itemsSource = pointItem.RealType.GetEnumValues();
                var enumerable = itemsSource.Cast<Object>().ToList().AsReadOnly();
                var comboBox = new ComboBox()
                {
                    ItemsSource = enumerable,
                    SelectedIndex = 0
                };
                var bingding2 = new Binding();
                bingding2.Path = "InputObject";
                comboBox.Bind(ComboBox.SelectedValueProperty, bingding2);
                stackPanel.Children.Add(comboBox);
                return stackPanel;
            }

            switch (pointItem.RealType.FullName!)
            {
                case "System.String":
                    return Templates["StringTemplate"].Build(item);
                case "System.Int32":
                    return Templates["IntTemplate"].Build(item);
                case "System.Double":
                    return Templates["DoubleTemplate"].Build(item);
                case "System.Boolean":
                    return Templates["BoolTemplate"].Build(item);
                case "PluginCore.SearchViewItem":
                    return Templates["SearchItemTemplate"].Build(item);
                default:
                    return Templates["InputTemplate"].Build(item);
            }
        }

        return null;
    }

    public bool Match(object? data) => true;
}