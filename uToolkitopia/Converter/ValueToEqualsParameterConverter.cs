#region

using System;
using System.Globalization;
using System.Windows.Data;

#endregion

namespace Kitopia.Converter;

public class ValueToEqualsParameterConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value == parameter;

    public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => null;
}