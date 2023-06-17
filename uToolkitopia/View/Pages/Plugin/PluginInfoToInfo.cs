using System;
using System.Globalization;
using System.Windows.Data;
using Core.SDKs.Services.Plugin;

namespace Kitopia.View.Pages.Plugin;

public class PluginInfoToInfo : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not PluginInfoEx)
            return null;
        var item = (PluginInfoEx)value;
        return $"ID:{item.PluginId},作者:{item.Author},版本:{item.Version}";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}