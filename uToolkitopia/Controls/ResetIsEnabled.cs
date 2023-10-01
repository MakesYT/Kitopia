#region

using System.Windows;
using System.Windows.Controls;

#endregion

namespace Kitopia.Controls;

public class ResetIsEnabled : ContentControl
{
    static ResetIsEnabled()
    {
        IsEnabledProperty.OverrideMetadata(
            typeof(ResetIsEnabled),
            new UIPropertyMetadata(
                true,
                (_, __) =>
                {
                },
                (_, x) => x));
    }
}