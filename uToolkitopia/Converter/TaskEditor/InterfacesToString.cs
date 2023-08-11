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
            for (int i = 0; i < list.Count; i++)
            {
                stringBuilder.Append(list[i]); // 追加对象
                if (i < list.Count - 1) // 如果不是最后一个对象
                {
                    stringBuilder.AppendLine(); // 添加换行符
                }
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