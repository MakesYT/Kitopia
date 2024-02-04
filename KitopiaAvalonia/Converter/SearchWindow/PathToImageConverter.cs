#region

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Core.SDKs.Tools;
using PluginCore;
using Bitmap = Avalonia.Media.Imaging.Bitmap;

#endregion

namespace Kitopia.Converter.SearchWindow;

public partial class PathToImageConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        //Console.WriteLine("开始获取  "+DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond );
        if (value is null)
        {
            return null;
        }

        var searchViewItem = value as SearchViewItem;


        if (searchViewItem is { Icon: null })
        {
            //Console.WriteLine("开始获取2 "+DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond );
            IconTools.GetIconInItems(searchViewItem);
            //.WriteLine("完成获取2 "+DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond );
        }

        switch (searchViewItem.FileType)
        {
            case FileType.命令:
            case FileType.自定义情景:
            case FileType.便签:
            case FileType.数学运算:
            case FileType.剪贴板图像:
            case FileType.None:
                return null;
            case FileType.文件夹:
            case FileType.自定义:
            case FileType.UWP应用:
            case FileType.应用程序:
            case FileType.Word文档:
            case FileType.PPT文档:
            case FileType.Excel文档:
            case FileType.PDF文档:
            case FileType.图像:
            case FileType.文件:
            case FileType.URL:

            default:
                break;
        }

        try
        {
            if (searchViewItem != null)
            {
                var icon = searchViewItem.Icon;
                
                System.Drawing.Bitmap bitmapTmp = icon.ToBitmap();
                var bitmapdata = bitmapTmp.LockBits(new Rectangle(0, 0, bitmapTmp.Width, bitmapTmp.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
                Bitmap bitmap1 = new Bitmap(Avalonia.Platform.PixelFormat.Bgra8888, Avalonia.Platform.AlphaFormat.Unpremul,
                    bitmapdata.Scan0,
                    new Avalonia.PixelSize(bitmapdata.Width, bitmapdata.Height),
                    new Avalonia.Vector(96, 96),
                    bitmapdata.Stride);
                bitmapTmp.UnlockBits(bitmapdata);
                bitmapTmp.Dispose();
                //Console.WriteLine("完成获取  "+DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond );
                return bitmap1;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(1);
            return null;
        }

        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}