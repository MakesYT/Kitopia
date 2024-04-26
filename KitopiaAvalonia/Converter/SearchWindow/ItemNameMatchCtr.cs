using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media;
using PluginCore;

namespace Kitopia.Converter.SearchWindow;

public class ItemNameMatchCtr : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var searchViewItem =
            ((Control)((Binding)parameter).DefaultAnchor.Target).DataContext as SearchViewItem;
        if (searchViewItem is not SearchViewItem str)
        {
            return new InlineCollection();
        }
        InlineCollection list = new();
        if (str.CharMatchResults.Length !=str.SplitWords.Length)
        {
            list.Add(new Run(str.ItemDisplayName));
            return list;
        }
        
        for (int i = 0; i < str.SplitWords.Length; i++)
        {
            
            list.Add(new Run(str.SplitWords[i].ToString())
            {
                Foreground = str.CharMatchResults[i] ? Brushes.OrangeRed : Brushes.Black,
            });
            
            
        }
        return list;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}