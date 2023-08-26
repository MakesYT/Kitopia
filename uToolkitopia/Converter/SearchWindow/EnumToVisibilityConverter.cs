#region

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Core.SDKs;

#endregion

namespace Kitopia.Converter.SearchWindow;

public class EnumToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null || parameter == null)
        {
            return Visibility.Collapsed;
        }

        switch (parameter)
        {
            case "RunAsAdmin":
            {
                switch ((FileType)value)
                {
                    case FileType.应用程序: return Visibility.Visible;
                    case FileType.命令: return Visibility.Visible;
                    case FileType.UWP应用: return Visibility.Visible;
                    default: return Visibility.Collapsed;
                }
            }
            case "Folder":
            {
                switch ((FileType)value)
                {
                    case FileType.应用程序: return Visibility.Visible;
                    case FileType.Word文档: return Visibility.Visible;
                    case FileType.PPT文档: return Visibility.Visible;
                    case FileType.Excel文档: return Visibility.Visible;
                    case FileType.PDF文档: return Visibility.Visible;
                    case FileType.图像: return Visibility.Visible;
                    case FileType.文件: return Visibility.Visible;
                    default: return Visibility.Collapsed;
                }
            }
            case "Console":
            {
                switch ((FileType)value)
                {
                    case FileType.应用程序: return Visibility.Visible;
                    case FileType.Word文档: return Visibility.Visible;
                    case FileType.PPT文档: return Visibility.Visible;
                    case FileType.Excel文档: return Visibility.Visible;
                    case FileType.PDF文档: return Visibility.Visible;
                    case FileType.图像: return Visibility.Visible;
                    case FileType.文件夹: return Visibility.Visible;
                    case FileType.文件: return Visibility.Visible;
                    default: return Visibility.Collapsed;
                }
            }
            case "Star":
            {
                switch ((FileType)value)
                {
                    case FileType.文件: return Visibility.Visible;
                    case FileType.文件夹: return Visibility.Visible;
                    default: return Visibility.Collapsed;
                }
            }
            case "Pin":
            {
                switch ((FileType)value)
                {
                    case FileType.剪贴板图像: return Visibility.Hidden;
                    case FileType.数学运算: return Visibility.Hidden;
                    default: return Visibility.Visible;
                }
            }
            default: return Visibility.Collapsed;
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}