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
        if (str.PinyinItem ==null||str.PinyinItem.CharMatchResults.Length-str.PinyinItem.ZhongWenCount !=str.PinyinItem.SplitWords.Length|| str.PinyinItem.CharMatchResults.Length==0)
        {
            list.Add(new Run(str.ItemDisplayName));
            return list;
        }
        
        
        for (int i = 0; i < str.PinyinItem.SplitWords.Length; i++)
        {
            
            list.Add(new Run(str.PinyinItem.SplitWords[i].ToString())
            {
                Foreground = str.PinyinItem.CharMatchResults[i+str.PinyinItem.ZhongWenCount] ? Brushes.OrangeRed : Brushes.Black,
            });
            
            
        }
        return list;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}