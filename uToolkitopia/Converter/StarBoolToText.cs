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

        if (parameter.Equals("Star"))
        {
            if ((bool)value)
            {
                return "取消收藏";
            }

            return "收藏";
        }

        if (parameter.Equals("Pin"))
        {
            if ((bool)value)
            {
                return "取消常驻";
            }

            return "常驻";
        }

        return "";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return (bool)!(bool)value;
    }
}