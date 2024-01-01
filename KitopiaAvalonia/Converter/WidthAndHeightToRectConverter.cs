using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace Kitopia.Converter;

public class WidthAndHeightToRectConverter : IMultiValueConverter
{
    public object Convert(IList<object?> values, Type targetType, object parameter, CultureInfo culture)
    {
        double width = (double)values[0];
        double height = (double)values[1];
        return new Rect(0, 0, width, height);
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter,
        System.Globalization.CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}