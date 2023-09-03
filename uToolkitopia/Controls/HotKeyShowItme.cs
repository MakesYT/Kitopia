#region

using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Wpf.Ui.Controls;
using Wpf.Ui.Converters;
using Button = System.Windows.Controls.Button;

#endregion

namespace Kitopia.Controls;

/// <summary>
///     Inherited from the <see cref="System.Windows.Controls.Button" />, adding <see cref="SymbolRegular" />.
/// </summary>
public class HotKeyShowItme : Button, IAppearanceControl
{
    /// <summary>
    ///     Property for <see cref="Icon" />.
    /// </summary>
    public static readonly DependencyProperty IconProperty = DependencyProperty.Register(nameof(Icon),
        typeof(IconElement), typeof(HotKeyShowItme),
        new PropertyMetadata(null, null, IconSourceElementConverter.ConvertToIconElement));

    /// <summary>
    ///     Property for <see cref="Appearance" />.
    /// </summary>
    public static readonly DependencyProperty AppearanceProperty = DependencyProperty.Register(nameof(Appearance),
        typeof(ControlAppearance), typeof(HotKeyShowItme),
        new PropertyMetadata(ControlAppearance.Primary));

    /// <summary>
    ///     Property for <see cref="MouseOverBackground" />.
    /// </summary>
    public static readonly DependencyProperty MouseOverBackgroundProperty = DependencyProperty.Register(
        nameof(MouseOverBackground),
        typeof(Brush), typeof(HotKeyShowItme),
        new PropertyMetadata(Border.BackgroundProperty.DefaultMetadata.DefaultValue));

    /// <summary>
    ///     Property for <see cref="MouseOverBorderBrush" />.
    /// </summary>
    public static readonly DependencyProperty MouseOverBorderBrushProperty = DependencyProperty.Register(
        nameof(MouseOverBorderBrush),
        typeof(Brush), typeof(HotKeyShowItme),
        new PropertyMetadata(Border.BorderBrushProperty.DefaultMetadata.DefaultValue));

    /// <summary>
    ///     Property for <see cref="PressedForeground" />.
    /// </summary>
    public static readonly DependencyProperty PressedForegroundProperty = DependencyProperty.Register(
        nameof(PressedForeground),
        typeof(Brush), typeof(HotKeyShowItme), new FrameworkPropertyMetadata(SystemColors.ControlTextBrush,
            FrameworkPropertyMetadataOptions.Inherits));

    /// <summary>
    ///     Property for <see cref="PressedBackground" />.
    /// </summary>
    public static readonly DependencyProperty PressedBackgroundProperty = DependencyProperty.Register(
        nameof(PressedBackground),
        typeof(Brush), typeof(HotKeyShowItme),
        new PropertyMetadata(Border.BackgroundProperty.DefaultMetadata.DefaultValue));

    /// <summary>
    ///     Property for <see cref="PressedBorderBrush" />.
    /// </summary>
    public static readonly DependencyProperty PressedBorderBrushProperty = DependencyProperty.Register(
        nameof(PressedBorderBrush),
        typeof(Brush), typeof(HotKeyShowItme),
        new PropertyMetadata(Border.BorderBrushProperty.DefaultMetadata.DefaultValue));

    /// <summary>
    ///     Gets or sets displayed <see cref="IconElement" />.
    /// </summary>
    [Bindable(true)]
    [Category("Appearance")]
    public IconElement? Icon
    {
        get => (IconElement)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <inheritdoc />
    [Bindable(true)]
    [Category("Appearance")]
    public ControlAppearance Appearance
    {
        get => (ControlAppearance)GetValue(AppearanceProperty);
        set => SetValue(AppearanceProperty, value);
    }

    /// <summary>
    ///     Background <see cref="Brush" /> when the user interacts with an element with a pointing device.
    /// </summary>
    [Bindable(true)]
    [Category("Appearance")]
    public Brush MouseOverBackground
    {
        get => (Brush)GetValue(MouseOverBackgroundProperty);
        set => SetValue(MouseOverBackgroundProperty, value);
    }

    /// <summary>
    ///     Border <see cref="Brush" /> when the user interacts with an element with a pointing device.
    /// </summary>
    [Bindable(true)]
    [Category("Appearance")]
    public Brush MouseOverBorderBrush
    {
        get => (Brush)GetValue(MouseOverBorderBrushProperty);
        set => SetValue(MouseOverBorderBrushProperty, value);
    }

    /// <summary>
    ///     Foreground when pressed.
    /// </summary>
    [Bindable(true)]
    [Category("Appearance")]
    public Brush PressedForeground
    {
        get => (Brush)GetValue(PressedForegroundProperty);
        set => SetValue(PressedForegroundProperty, value);
    }

    /// <summary>
    ///     Background <see cref="Brush" /> when the user clicks the button.
    /// </summary>
    [Bindable(true)]
    [Category("Appearance")]
    public Brush PressedBackground
    {
        get => (Brush)GetValue(PressedBackgroundProperty);
        set => SetValue(PressedBackgroundProperty, value);
    }

    /// <summary>
    ///     Border <see cref="Brush" /> when the user clicks the button.
    /// </summary>
    [Bindable(true)]
    [Category("Appearance")]
    public Brush PressedBorderBrush
    {
        get => (Brush)GetValue(PressedBorderBrushProperty);
        set => SetValue(PressedBorderBrushProperty, value);
    }
}