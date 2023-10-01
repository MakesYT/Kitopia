#region

using System.ComponentModel;
using System.Windows;
using Wpf.Ui.Controls;

#endregion

namespace Kitopia.Controls;

public class HotKeyShow : Button
{
    public enum KeyTypeE
    {
        None = 0000,
        CtrlAlt = 1010,
        Ctrl = 1000,
        CtrlShift = 1100,
        CtrlShiftAlt = 1110,
        Alt = 0010,
        AltShift = 0110,
        Win = 0001,
        WinShift = 0101,
        WinAlt = 0011,
        Shift = 0100
    }

    public static readonly DependencyProperty KeyTypeProperty = DependencyProperty.Register(nameof(KeyType),
        typeof(KeyTypeE), typeof(System.Windows.Controls.Button),
        new PropertyMetadata(KeyTypeE.Alt));

    public static readonly DependencyProperty KeyNameProperty = DependencyProperty.Register(nameof(KeyName),
        typeof(string), typeof(System.Windows.Controls.Button),
        new PropertyMetadata("空格"));

    [Bindable(true)]
    [Category("KeyType")]
    public KeyTypeE KeyType
    {
        get => (KeyTypeE)GetValue(KeyTypeProperty);
        set => SetValue(KeyTypeProperty, value);
    }

    [Bindable(true)]
    [Category("KeyName")]
    public string KeyName
    {
        get => (string)GetValue(KeyNameProperty);
        set => SetValue(KeyNameProperty, value);
    }
}