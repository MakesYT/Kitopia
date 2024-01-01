﻿#region

using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;
using Vanara.Windows.Shell;

#endregion

namespace Kitopia.Converter.TaskEditor;

public class IfNullVisibilityHidden : IValueConverter

{
    public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null)
        {
            return Visibility.Collapsed;
        }

        if (value is List<string> { Count: 0 })
        {
            return Visibility.Collapsed;
        }

        if (value is string s && string.IsNullOrEmpty(s))
        {
            return Visibility.Collapsed;
        }

        return Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}