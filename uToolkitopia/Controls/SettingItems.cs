#region

using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Wpf.Ui.Controls;
using Wpf.Ui.Converters;

#endregion

namespace Kitopia.Controls;

public class SettingItems : ContentControl
{
    [Bindable(true)]
    [Category("Title")]
    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    [Bindable(true)] public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
        nameof(Title),
        typeof(string), typeof(SettingItems),
        new FrameworkPropertyMetadata(""));

    [Bindable(true)]
    [Category("Description")]
    public string Description
    {
        get => (string)GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register(nameof(Description),
        typeof(string), typeof(SettingItems),
        new PropertyMetadata(""));


    [Bindable(true)]
    [Category("Appearance")]
    public IconElement? Icon
    {
        get => (IconElement)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public static readonly DependencyProperty IconProperty = DependencyProperty.Register(nameof(Icon),
        typeof(IconElement), typeof(SettingItems),
        new PropertyMetadata(null, null, IconSourceElementConverter.ConvertToIconElement));
}