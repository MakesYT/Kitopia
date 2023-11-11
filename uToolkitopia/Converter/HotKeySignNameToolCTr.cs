using System;
using System.Globalization;
using System.Windows.Data;

namespace Kitopia.Converter;

public class HotKeySignNameToolCTr : IMultiValueConverter

{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        return string.Join("_", values);
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}