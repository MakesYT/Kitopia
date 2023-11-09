#region

using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Data;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using Core.SDKs;
using Core.SDKs.Services;
using Core.ViewModel;

#endregion

namespace Kitopia.Converter.SearchWindow;

public partial class PathToImageConverter : IValueConverter
{
    private SearchWindowViewModel? SearchWindowViewModel;

    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        //Console.WriteLine("开始获取  "+DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond );
        SearchWindowViewModel ??=
            ((SearchWindowViewModel)ServiceManager.Services.GetService(typeof(SearchWindowViewModel)));
        var searchViewItem = value as SearchViewItem;
        if (searchViewItem.Icon is null)
        {
            //Console.WriteLine("开始获取2 "+DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond );
            SearchWindowViewModel.GetIconInItemsAsync(searchViewItem);
            //.WriteLine("完成获取2 "+DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond );
            return null;
        }

        try
        {
            var icon = searchViewItem.Icon;
            var hIcon = icon.Handle;
            var source = Imaging.CreateBitmapSourceFromHIcon(
                hIcon,
                new Int32Rect(0, 0, icon.Width, icon.Height),
                BitmapSizeOptions.FromEmptyOptions());
            //Console.WriteLine("完成获取  "+DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond );
            return source;
        }
        catch (Exception e)
        {
            Console.WriteLine(1);
            return null;
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotImplementedException();

    [LibraryImport("gdi32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool DeleteObject(IntPtr hObject);
}