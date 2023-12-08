﻿using System;
using System.Globalization;
using System.Windows.Data;
using Core.ViewModel;
using PluginCore;

namespace Kitopia.Converter;

public class MouseQuickSelectedItem : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is SelectedItem item)
        {
            switch (item.type)
            {
                case FileType.文本:
                {
                    return $"当前选中{((string)item.obj).Length}个文字";
                }
            }
        }

        return "当前未选中文字";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}