﻿using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using Core.SDKs.Config;

namespace Kitopia.Converter;

public class HotKeysToKeyName : IValueConverter
{
    public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        var hotKeyModel = ConfigManger.config.hotKeys.FirstOrDefault(e =>
        {
            if (e.Name.Equals(parameter))
            {
                return true;
            }

            return false;
        });
        return hotKeyModel.SelectKey.ToString();
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return null;
    }
}