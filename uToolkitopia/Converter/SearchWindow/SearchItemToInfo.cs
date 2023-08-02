﻿using System;
using System.Globalization;
using System.Text;
using System.Windows.Data;
using Core.SDKs;

namespace Kitopia.Converter.SearchWindow;

public class SearchItemToInfo : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not SearchViewItem)
            return null;
        var item = (SearchViewItem)value;
        StringBuilder info = new StringBuilder();
        switch (item.FileType)
        {
            case FileType.文件夹:
                info.Append("名称:");
                info.AppendLine(item.DirectoryInfo.Name);
                info.Append("位置:");
                info.Append(item.DirectoryInfo.FullName);
                break;
            case FileType.命令:
            case FileType.URL:
                info.Append("名称:");
                info.AppendLine(item.FileName);
                info.Append("目标:");
                info.Append(item.OnlyKey);
                break;
            case FileType.数学运算:
            case FileType.剪贴板图像:
            case FileType.None:
                break;
            case FileType.应用程序:
            case FileType.Word文档:
            case FileType.PPT文档:
            case FileType.Excel文档:
            case FileType.PDF文档:
            case FileType.图像:
            case FileType.文件:
            {
                info.Append("名称:");
                info.AppendLine(item.FileInfo.Name);
                info.Append("位置:");
                info.Append(item.FileInfo.FullName);
                break;
            }
            default:
                return null;
        }

        if (info.Length > 1)
        {
            return info.ToString();
        }

        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}