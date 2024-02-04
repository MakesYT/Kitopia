using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Kitopia.Converter.SearchWindow;

public class SymbolToVisible : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return (int)value! != 0;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
}