#region

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using Core.SDKs.Services.Plugin;
using Wpf.Ui.Controls;

#endregion

namespace Kitopia.View.Pages.Plugin;

public class SettingItemToControlConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var setting = value as PluginSettingItem;
        if (setting == null)
        {
            return null;
        }

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
            binding.Path = new PropertyPath("Setting");
            binding.Source = this;
            switch (control)
            {
                case ToggleSwitch:
                    control.SetBinding(ToggleButton.IsCheckedProperty, binding);
                    break;
                case ComboBox:
                    control.SetBinding(Selector.SelectedValueProperty, binding);
                    break;
            }

            return control;
        }

        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}