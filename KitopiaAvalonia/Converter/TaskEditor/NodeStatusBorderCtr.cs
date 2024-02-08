﻿using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Core.SDKs.CustomScenario;

namespace Kitopia.Converter.TaskEditor;

public class NodeStatusBorderCtr : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not s节点状态 status || parameter is null)
        {
            return false;
        }

        if (((int)status).ToString() == parameter.ToString())
        {
            return true;
        }

        return false;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}