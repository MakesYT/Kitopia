using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Kitopia.Converter;

public class ReverseBool : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        
        return (bool)!(bool)value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return (bool)!(bool)value;
    }
}