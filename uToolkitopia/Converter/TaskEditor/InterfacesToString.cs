using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;

namespace Kitopia.Converter.TaskEditor;

public class InterfacesToString : IValueConverter
{
    public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is null)
        {
            return "";
        }

        if (value is List<string> list)
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (var s in list)
            {
                stringBuilder.AppendLine(s);
            }

            return stringBuilder.ToString();
        }

        return "";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return (bool)!(bool)value;
    }
}