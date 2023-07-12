using System;
using System.Drawing;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Data;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace Kitopia.Converter.SearchWindow;

public class PathToImageConverter : IValueConverter
{
    [DllImport("gdi32.dll")]
    public static extern bool DeleteObject(IntPtr hObject);

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null)
            return null;
        var icon = value as Icon;
        try
        {
            IntPtr hIcon = icon.Handle;
            var source = Imaging.CreateBitmapSourceFromHIcon(
                hIcon,
                new Int32Rect(0, 0, icon.Width, icon.Height),
                BitmapSizeOptions.FromEmptyOptions());
            DeleteObject(hIcon); // 释放图标句柄
            return source;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return null;
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}