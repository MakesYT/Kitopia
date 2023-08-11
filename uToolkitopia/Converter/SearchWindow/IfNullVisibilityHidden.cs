using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Kitopia.Converter.SearchWindow;

public class IfNullVisibilityHidden : IValueConverter

{
    public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null || (int)value == 0)
        {
            return Visibility.Collapsed;
        }

        return Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}