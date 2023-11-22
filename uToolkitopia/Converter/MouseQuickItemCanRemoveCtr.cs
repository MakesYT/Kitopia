using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using PluginCore;

namespace Kitopia.Converter;

public class MouseQuickItemCanRemoveCtr : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is FileType fileType && fileType != FileType.None)
        {
            return Visibility.Visible;
        }
        else
        {
            return Visibility.Collapsed;
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}