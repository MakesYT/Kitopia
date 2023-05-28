using System;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Data;
using Core.SDKs;

namespace Kitopia.Converter;

public class SearchItemToInfo : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not SearchViewItem)
            return null;
        var item = (SearchViewItem)value;
        StringBuilder info = new StringBuilder();
        if (item.DirectoryInfo is not null)
        {
            info.Append("名称:");
            info.AppendLine(item.DirectoryInfo.Name);
            info.Append("位置:");
            info.Append(item.DirectoryInfo.FullName);
        }
        else if (item.FileInfo is not null)
        {
            info.Append("名称:");
            info.AppendLine(item.FileInfo.Name);
            info.Append("位置:");
            info.Append(item.FileInfo.FullName);
        }

        return info;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}