using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Core.SDKs.Services.Plugin;
using Wpf.Ui.Controls;

namespace Kitopia.View.Pages.Plugin;

public class SettingItemToControlConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var setting = value as PluginSettingItem;
        if (setting == null) return null;

        FrameworkElement control;
        switch (setting.ControlType)
        {
            case "ToggleSwitch":
                control = new ToggleSwitch();
                break;
            case "ComboBox":
                control = new ComboBox();
                break;
            default:
                control = null;
                break;
        }

        if (control != null)
        {
            var binding = new Binding();
            binding.Path = new PropertyPath(setting.SettingName);
            binding.Source = this;
            switch (control)
            {
                case ToggleSwitch:
                    control.SetBinding(ToggleSwitch.IsCheckedProperty, binding);
                    break;
                case ComboBox:
                    control.SetBinding(ComboBox.SelectedValueProperty, binding);
                    break;
            }

            return control;
        }
        else
        {
            return null;
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}