#region

using System;
using System.Globalization;
using Avalonia.Data.Converters;
using PluginCore;

#endregion

namespace Kitopia.View.Pages.Plugin;

public class PluginInfoToInfo : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not PluginInfo)
        {
            return null;
        }

        var item = (PluginInfo)value;
        return $"ID:{item.PluginId},作者:{item.Author},版本:{item.Version}";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}