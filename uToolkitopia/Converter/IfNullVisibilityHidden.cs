using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Wpf.Ui.Common;

namespace Kitopia.Converter;

public class IfNullVisibilityHidden:IValueConverter

{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if ((int)value ==0)
        {
            return Visibility.Hidden;
        }
        return Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}