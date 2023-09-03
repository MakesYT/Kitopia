#region

using System;
using System.Globalization;
using System.Windows.Data;
using Wpf.Ui.Controls;

#endregion

namespace Kitopia.Converter;

public class IntToIconSymbol : IValueConverter

{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        (SymbolRegular)(int)value;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}