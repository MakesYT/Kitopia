using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using Core.SDKs.Services.Config;
using Kitopia.Controls;

namespace Kitopia.Converter;

public class HotKeysToHotKeyShow : IValueConverter
{
    public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        var hotKeyModel = ConfigManger.Config.hotKeys.FirstOrDefault(e =>
        {
            if ($"{e.MainName}_{e.Name}".Equals(parameter))
            {
                return true;
            }

            return false;
        });
        int type = 0000;
        if (hotKeyModel.IsSelectAlt)
        {
            type += 10;
        }

        if (hotKeyModel.IsSelectCtrl)
        {
            type += 1000;
        }

        if (hotKeyModel.IsSelectShift)
        {
            type += 100;
        }

        if (hotKeyModel.IsSelectWin)
        {
            type += 1;
        }

        return (HotKeyShow.KeyTypeE)type;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return null;
    }
}