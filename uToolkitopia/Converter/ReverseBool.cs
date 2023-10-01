#region

using System;
using System.Globalization;
using System.Windows.Data;

#endregion

namespace Kitopia.Converter;

public class ReverseBool : IValueConverter
{
    public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is null)
        {
            return false;
        }

        return !(bool)value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        !(bool)value;
}