#region

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using PluginCore;

#endregion

namespace Kitopia.Converter.SearchWindow;

public class CanIgnoreCtr : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null)
        {
            return Visibility.Collapsed;
        }

        switch ((FileType)value)
        {
            case FileType.应用程序: return Visibility.Visible;
            case FileType.UWP应用: return Visibility.Visible;
            case FileType.Word文档: return Visibility.Visible;
            case FileType.PPT文档: return Visibility.Visible;
            case FileType.Excel文档: return Visibility.Visible;
            case FileType.PDF文档: return Visibility.Visible;
            case FileType.图像: return Visibility.Visible;
            case FileType.文件: return Visibility.Visible;
            default: return Visibility.Collapsed;
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}