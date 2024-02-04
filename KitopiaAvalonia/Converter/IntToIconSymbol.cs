#region

using System;
using System.Globalization;
using Avalonia.Data.Converters;
using FluentAvalonia.UI.Controls;

#endregion

namespace Kitopia.Converter;

public class IntToIconSymbol : IValueConverter

{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return $"&#x{((int)value).ToString("X")};";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}