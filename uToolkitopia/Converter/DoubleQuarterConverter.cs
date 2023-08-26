﻿#region

using System;
using System.Globalization;
using System.Windows.Data;

#endregion

namespace Kitopia.Converter;

public class DoubleQuarterConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var doubleValue = (double)value;
        return doubleValue / 4;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}