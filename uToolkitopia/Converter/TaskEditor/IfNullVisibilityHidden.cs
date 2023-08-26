#region

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

#endregion

namespace Kitopia.Converter.TaskEditor;

public class IfNullVisibilityHidden : IValueConverter

{
    public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null || ((List<string>)value).Count == 0)
        {
            return Visibility.Collapsed;
        }

        return Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}