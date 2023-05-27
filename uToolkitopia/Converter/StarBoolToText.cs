using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Kitopia.Converter;

public class StarBoolToText : IValueConverter
{
    public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is null)
        {
            return "";
        }

        if ((bool)value)
        {
            return "取消收藏";
        }
        else
        {
            return "收藏";
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return (bool)!(bool)value;
    }
}