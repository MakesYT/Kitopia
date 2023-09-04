using System.Windows;
using System.Windows.Controls;

namespace Kitopia.Controls;

public class ResetIsEnabled : ContentControl
{
    static ResetIsEnabled()
    {
        IsEnabledProperty.OverrideMetadata(
            typeof(ResetIsEnabled),
            new UIPropertyMetadata(
                defaultValue: true,
                propertyChangedCallback: (_, __) =>
                {
                },
                coerceValueCallback: (_, x) => x));
    }
}