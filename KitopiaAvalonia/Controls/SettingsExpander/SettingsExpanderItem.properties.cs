﻿using System;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Templates;
using Avalonia.Interactivity;
namespace KitopiaAvalonia.Controls.SettingsExpander;

[PseudoClasses(s_pcFooterBottom, SharedPseudoclasses.s_pcFooter, s_pcContent, s_pcDescription)]
[PseudoClasses(SharedPseudoclasses.s_pcAllowClick)]
[PseudoClasses(SharedPseudoclasses.s_pcPressed)]
[PseudoClasses(SharedPseudoclasses.s_pcIcon, s_pcActionIcon)]
public partial class SettingsExpanderItem : ContentControl
{
    /// <summary>
    /// Defines the <see cref="Description"/> property
    /// </summary>
    public static readonly StyledProperty<string> DescriptionProperty =
        KitopiaAvalonia.Controls.SettingsExpander.SettingsExpander.DescriptionProperty.AddOwner<KitopiaAvalonia.Controls.SettingsExpander.SettingsExpanderItem>();

    /// <summary>
    /// Defines the <see cref="IconSource"/> property
    /// </summary>
    public static readonly StyledProperty<object> IconSourceProperty =
        KitopiaAvalonia.Controls.SettingsExpander.SettingsExpander.IconSourceProperty.AddOwner<KitopiaAvalonia.Controls.SettingsExpander.SettingsExpanderItem>();

    /// <summary>
    /// Defines the <see cref="Footer"/> property
    /// </summary>
    public static readonly StyledProperty<object> FooterProperty =
        KitopiaAvalonia.Controls.SettingsExpander.SettingsExpander.FooterProperty.AddOwner<KitopiaAvalonia.Controls.SettingsExpander.SettingsExpanderItem>();

    /// <summary>
    /// Defines the <see cref="FooterTemplate"/> property
    /// </summary>
    public static readonly StyledProperty<IDataTemplate> FooterTemplateProperty =
        KitopiaAvalonia.Controls.SettingsExpander.SettingsExpander.FooterTemplateProperty.AddOwner<KitopiaAvalonia.Controls.SettingsExpander.SettingsExpanderItem>();

    /// <summary>
    /// Defines the <see cref="ActionIconSource"/> property
    /// </summary>
    public static readonly StyledProperty<object> ActionIconSourceProperty =
        KitopiaAvalonia.Controls.SettingsExpander.SettingsExpander.ActionIconSourceProperty.AddOwner<KitopiaAvalonia.Controls.SettingsExpander.SettingsExpanderItem>();

    /// <summary>
    /// Defines the <see cref="IsClickEnabled"/> property
    /// </summary>
    public static readonly StyledProperty<bool> IsClickEnabledProperty =
        KitopiaAvalonia.Controls.SettingsExpander.SettingsExpander.IsClickEnabledProperty.AddOwner<KitopiaAvalonia.Controls.SettingsExpander.SettingsExpanderItem>();

    /// <summary>
    /// Defines the <see cref="Command"/> property
    /// </summary>
    public static readonly StyledProperty<ICommand> CommandProperty =
        Button.CommandProperty.AddOwner<KitopiaAvalonia.Controls.SettingsExpander.SettingsExpanderItem>();

    /// <summary>
    /// Defines the <see cref="CommandParameter"/> property
    /// </summary>
    public static readonly StyledProperty<object> CommandParameterProperty =
        Button.CommandParameterProperty.AddOwner<KitopiaAvalonia.Controls.SettingsExpander.SettingsExpanderItem>();

    /// <summary>
    /// Defines the <see cref="TemplateSettings"/> property
    /// </summary>
    public static readonly StyledProperty<SettingsExpanderTemplateSettings> TemplateSettingsProperty =
        AvaloniaProperty.Register<KitopiaAvalonia.Controls.SettingsExpander.SettingsExpanderItem, SettingsExpanderTemplateSettings>(nameof(TemplateSettings));

    /// <summary>
    /// Defines the <see cref="Click"/> event
    /// </summary>
    public static readonly RoutedEvent<RoutedEventArgs> ClickEvent =
        KitopiaAvalonia.Controls.SettingsExpander.SettingsExpander.ClickEvent;

    /// <summary>
    /// Gets or sets the description text
    /// </summary>
    public string Description
    {
        get => GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    /// <summary>
    /// Gets or sets the IconSource for the SettingsExpander
    /// </summary>
    public object IconSource
    {
        get => GetValue(IconSourceProperty);
        set => SetValue(IconSourceProperty, value);
    }

    /// <summary>
    /// Gets or sets the Footer content for the SettingsExpander
    /// </summary>
    public object Footer
    {
        get => GetValue(FooterProperty);
        set => SetValue(FooterProperty, value);
    }

    /// <summary>
    /// Gets or sets the Footer template for the SettingsExpander
    /// </summary>
    public IDataTemplate FooterTemplate
    {
        get => GetValue(FooterTemplateProperty);
        set => SetValue(FooterTemplateProperty, value);
    }

    /// <summary>
    /// Gets or sets the Action IconSource when <see cref="IsClickEnabled"/> is true
    /// </summary>
    public object ActionIconSource
    {
        get => GetValue(ActionIconSourceProperty);
        set => SetValue(ActionIconSourceProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the item is clickable which can be used for navigation within an app
    /// </summary>
    /// <remarks>
    /// This property can only be set if no items are added to the SettingsExpander. Attempting to mark
    /// a settings expander clickable and adding child items will throw an exception
    /// </remarks>
    public bool IsClickEnabled
    {
        get => GetValue(IsClickEnabledProperty);
        set => SetValue(IsClickEnabledProperty, value);
    }

    /// <summary>
    /// Gets or sets the Command that is invoked upon clicking the item
    /// </summary>
    public ICommand Command
    {
        get => GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the command parameter
    /// </summary>
    public object CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    /// <summary>
    /// Provides calculated values that can be referenced as TemplatedParent sources when defining 
    /// templates for a SettingsExpander. Not intended for general use.
    /// </summary>
    public SettingsExpanderTemplateSettings TemplateSettings
    {
        get => GetValue(TemplateSettingsProperty);
        private set => SetValue(TemplateSettingsProperty, value);
    }

    protected override bool IsEnabledCore => base.IsEnabledCore && _commandCanExecute;

    /// <summary>
    /// Event raised when the SettingsExpander is clicked and IsClickEnabled = true
    /// </summary>
    public event EventHandler<RoutedEventArgs> Click
    {
        add => AddHandler(ClickEvent, value);
        remove => RemoveHandler(ClickEvent, value);
    }

    internal bool IsContainerFromTemplate { get; set; }

    private const string s_pcDescription = ":description";
    private const string s_pcContent = ":content";
    private const string s_pcActionIcon = ":actionIcon";
    private const string s_pcFooterBottom = ":footerBottom";

    private const string s_resAdaptiveWidthTrigger = "SettingsExpanderItemAdaptiveWidthTrigger";
}