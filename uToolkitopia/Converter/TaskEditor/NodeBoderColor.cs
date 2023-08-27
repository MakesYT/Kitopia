using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Data;
using Core.ViewModel.TaskEditor;

namespace Kitopia.Converter.TaskEditor;

public class NodeBoderColor : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is s节点状态 va)
        {
            switch (va)
            {
                case s节点状态.未验证:
                    return new SolidBrush(Color.Gray);
                case s节点状态.已验证:
                    return new SolidBrush(Color.LawnGreen);
                case s节点状态.错误:
                    return new SolidBrush(Color.OrangeRed);
            }
        }

        return new SolidBrush(Color.Gray);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}