using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Core.SDKs.CustomScenario;

namespace Kitopia.Converter.TaskEditor;

public class AutoTriggersCtr : IValueConverter
{
    public CustomScenario CustomScenario;

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is CustomScenario customScenario)
        {
            CustomScenario = customScenario;
            //return customScenario.AutoTriggers.Contains((string)((BindingProxy)parameter).Data);
        }

        return false;
    }

    public object ConvertBack(object value, Type targetType, object? parameter, CultureInfo culture)
    {
        /*if (value is bool b)
        {
            var key = (string)((BindingProxy)parameter).Data;
            WeakReferenceMessenger.Default.Send(new CustomScenarioChangeMsg()
                { Type = 1, Name = key, CustomScenario = CustomScenario });

            if (b)
            {
                CustomScenario.AutoTriggers.Add(key);
            }
            else
            {
                if (CustomScenario.AutoTriggers.Contains(key))
                {
                    CustomScenario.AutoTriggers.Remove(key);
                }
            }
        }*/

        return CustomScenario;
    }
}