﻿using System;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Data;
using Avalonia.Data.Converters;
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
        if (str.PinyinItem == null || str.PinyinItem.CharMatchResults == null || str.PinyinItem.SplitWords == null ||
            str.PinyinItem.CharMatchResults.Length - str.PinyinItem.ZhongWenCount != str.PinyinItem.SplitWords.Length ||
            str.PinyinItem.CharMatchResults.Length == 0)
        {
            list.Add(new Run(str.ItemDisplayName));
            return list;
        }


        for (int i = 0; i < str.PinyinItem.SplitWords.Length; i++)
        {
            var inline = new Run(str.PinyinItem.SplitWords[i]);
            if (str.PinyinItem.CharMatchResults[i + str.PinyinItem.ZhongWenCount])
            {
                inline.Foreground = Brushes.OrangeRed;
            }

            list.Add(inline);
        }

        return list;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}