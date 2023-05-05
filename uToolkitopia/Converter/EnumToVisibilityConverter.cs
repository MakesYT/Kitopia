using Core.SDKs;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Kitopia.Converter
{
    public class EnumToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return Visibility.Collapsed;

            switch (parameter)
            {
                case "RunAsAdmin":
                    {
                        switch ((FileType)value)
                        {
                            case FileType.App: return Visibility.Visible;
                            default: return Visibility.Collapsed;
                        }
                    }
                case "Folder":
                    {
                        switch ((FileType)value)
                        {
                            case FileType.App: return Visibility.Visible;
                            case FileType.Word文档: return Visibility.Visible;
                            case FileType.PPT文档: return Visibility.Visible;
                            case FileType.Excel文档: return Visibility.Visible;
                            case FileType.PDF文档: return Visibility.Visible;
                            case FileType.图像: return Visibility.Visible;
                            default: return Visibility.Collapsed;
                        }
                    }
                case "Console":
                    {
                        switch ((FileType)value)
                        {
                            case FileType.App: return Visibility.Visible;
                            case FileType.Word文档: return Visibility.Visible;
                            case FileType.PPT文档: return Visibility.Visible;
                            case FileType.Excel文档: return Visibility.Visible;
                            case FileType.PDF文档: return Visibility.Visible;
                            case FileType.图像: return Visibility.Visible;
                            default: return Visibility.Collapsed;
                        }
                    }
                default: return Visibility.Collapsed;

            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
