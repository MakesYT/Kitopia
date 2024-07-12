﻿#region

using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Metadata;
using Core.SDKs.CustomScenario;
using KitopiaAvalonia.Tools;

#endregion

namespace KitopiaAvalonia.Pages;

public class MyDataTemplateSelector : IDataTemplate
{
    // Override the SelectTemplate method
    [Content] public Dictionary<string, IDataTemplate> Templates { get; } = new Dictionary<string, IDataTemplate>();

    private static Type ResolveType(string? ns, string typeName)
    {
        return typeName switch
        {
            "TextBlock" => typeof(TextBlock),
            "ComboBox" => typeof(ComboBox),
            _ => throw new InvalidOperationException($"Could not resolve type {typeName}.")
        };
    }

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
                var control = Templates["EnumTemplate"].Build(item);
                var childOfType = control.GetChildOfType<ComboBox>("ComboBox");
                childOfType.ItemsSource = pointItem.RealType.GetEnumValues();
                Console.WriteLine(1);
                return control;
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