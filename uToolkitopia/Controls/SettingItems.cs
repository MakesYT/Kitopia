// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.ComponentModel;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;

namespace Kitopia.Controls;


public class SettingItems : ContentControl
{
    [Bindable(true), Category("Title")]
    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }
    public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(nameof(Title),
        typeof(string), typeof(TextBlock),
        new PropertyMetadata(""));
    [Bindable(true), Category("Title")]
    public string Description
    {
        get => (string)GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }
    public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register(nameof(Description),
        typeof(string), typeof(TextBlock),
        new PropertyMetadata(""));
    
}
