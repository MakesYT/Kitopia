using System;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media.Imaging;
using Core.ViewModel.Pages;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PluginCore;


namespace KitopiaAvalonia.Converter.PluginManagerPage;

public class IconCtr : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not null)
        {
            return value;
        }
        var onlinePluginInfo =
            ((Control)((CompiledBindingExtension)parameter).DefaultAnchor.Target).DataContext as PluginInfo;
        {
            if (onlinePluginInfo.Icon is not null)
            {
                return onlinePluginInfo.Icon;
            }

            Task.Run(async () =>
            {
                onlinePluginInfo.Icon = new Bitmap($"{onlinePluginInfo.Path}{Path.DirectorySeparatorChar}avatar.png");
            });


        }

        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}