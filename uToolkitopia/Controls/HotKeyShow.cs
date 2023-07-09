// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace Kitopia.Controls;

public class HotKeyShow : Wpf.Ui.Controls.Button
{
    public enum KeyTypeE
    {
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

    [Bindable(true), Category("KeyType")]
    public KeyTypeE KeyType
    {
        get => (KeyTypeE)GetValue(KeyTypeProperty);
        set => SetValue(KeyTypeProperty, value);
    }

    public static readonly DependencyProperty KeyTypeProperty = DependencyProperty.Register(nameof(KeyType),
        typeof(KeyTypeE), typeof(Button),
        new PropertyMetadata(KeyTypeE.Alt));

    [Bindable(true), Category("KeyName")]
    public string KeyName
    {
        get => (string)GetValue(KeyNameProperty);
        set => SetValue(KeyNameProperty, value);
    }

    public static readonly DependencyProperty KeyNameProperty = DependencyProperty.Register(nameof(KeyName),
        typeof(string), typeof(Button),
        new PropertyMetadata("空格"));
}