#region

using System.Windows;

#endregion

namespace Kitopia.Services;

public class BindingProxy : Freezable
{
    protected override Freezable CreateInstanceCore() => new BindingProxy();

    public object Data
    {
        get => (object)GetValue(DataProperty);
        set => SetValue(DataProperty, value);
    }

    // Using a DependencyProperty as the backing store for Data.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty DataProperty =
        DependencyProperty.Register("Data", typeof(object), typeof(BindingProxy), new PropertyMetadata(null));
}