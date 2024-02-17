using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Styling;
using Core.SDKs.Services;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Navigation;
using FluentAvalonia.UI.Windowing;
using KitopiaAvalonia.Pages;
using log4net;
using Microsoft.Extensions.DependencyInjection;

namespace KitopiaAvalonia;

public class NavigationPageFactory : INavigationPageFactory
{
    public Control GetPage(Type srcType) => throw new NotImplementedException();

    public Control GetPageFromObject(object target)
    {
        if (target is string s)
        {
            return ServiceManager.Services.GetKeyedService<UserControl>(s);
        }

        return null;
    }
}

public partial class MainWindow : AppWindow
{
    private static readonly ILog log = LogManager.GetLogger(nameof(MainWindow));

    public MainWindow()
    {
        InitializeComponent();
        RenderOptions.SetTextRenderingMode(this, TextRenderingMode.Antialias);
        TitleBar.ExtendsContentIntoTitleBar = true;
        TitleBar.TitleBarHitTestType = TitleBarHitTestType.Complex;
        FrameView.NavigationPageFactory = new NavigationPageFactory();
        foreach (NavigationViewItem nvi in NavView.MenuItems)
        {
            if (nvi.Tag == FrameView.Tag)
            {
                NavView.SelectedItem = nvi;
                SetNVIIcon(nvi, true);
            }
            else
            {
                SetNVIIcon(nvi, false);
            }
        }

        foreach (NavigationViewItem nvi in NavView.FooterMenuItems)
        {
            if (nvi.Tag == FrameView.Tag)
            {
                NavView.SelectedItem = nvi;
                SetNVIIcon(nvi, true);
            }
            else
            {
                SetNVIIcon(nvi, false);
            }
        }

        IsVisible = false;
    }

    private IReadOnlyDictionary<Type, string> Pages => new Dictionary<Type, string>
    {
        { typeof(HomePage), "HomePage" },
        { typeof(PluginManagerPage), "PluginManagerPage" },
        { typeof(CustomScenariosManagerPage), "CustomScenariosManagerPage" },
        { typeof(HotKeyManagerPage), "HotKeyManagerPage" },
        { typeof(SettingPage), "SettingPage" }
    };


    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        if (VisualRoot is AppWindow aw)
        {
            TitleBarHost.ColumnDefinitions[3].Width = new GridLength(aw.TitleBar.RightInset, GridUnitType.Pixel);
        }
    }

    private void Window_OnClosing(object? sender, WindowClosingEventArgs e)
    {
        this.IsVisible = false;
        e.Cancel = true;
    }

    private void NavView_OnItemInvoked(object? sender, NavigationViewItemInvokedEventArgs e)
    {
        FrameView.NavigateFromObject(e.InvokedItemContainer.Tag);
    }

    protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnAttachedToLogicalTree(e);
    }

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        //
    }

    private void FrameView_OnNavigated(object sender, NavigationEventArgs e)
    {
        FrameView.Tag = Pages.GetValueOrDefault(e.Content.GetType());
        foreach (NavigationViewItem nvi in NavView.MenuItems)
        {
            if (nvi.Tag == FrameView.Tag)
            {
                NavView.SelectedItem = nvi;
                SetNVIIcon(nvi, true);
            }
            else
            {
                SetNVIIcon(nvi, false);
            }
        }

        foreach (NavigationViewItem nvi in NavView.FooterMenuItems)
        {
            if (nvi.Tag == FrameView.Tag)
            {
                NavView.SelectedItem = nvi;
                SetNVIIcon(nvi, true);
            }
            else
            {
                SetNVIIcon(nvi, false);
            }
        }

        if (FrameView.BackStackDepth > 0 && !NavView.IsBackButtonVisible)
        {
            AnimateContentForBackButton(true);
        }
        else if (FrameView.BackStackDepth == 0 && NavView.IsBackButtonVisible)
        {
            AnimateContentForBackButton(false);
        }
    }

    private void SetNVIIcon(NavigationViewItem item, bool selected)
    {
        // Technically, yes you could set up binding and converters and whatnot to let the icon change
        // between filled and unfilled based on selection, but this is so much simpler 

        if (item == null)
            return;

        var t = item.Tag;
        switch (t)
        {
            case "HomePage":
            {
                item.IconSource = this.TryFindResource(selected ? "HomeIconFilled" : "HomeIcon", out var value)
                    ? (IconSource)value
                    : null;
                break;
            }
            case "PluginManagerPage":
            {
                item.IconSource = this.TryFindResource(selected ? "AppsIconFilled" : "AppsIcon", out var value)
                    ? (IconSource)value
                    : null;
                break;
            }
            case "CustomScenariosManagerPage":
            {
                item.IconSource =
                    this.TryFindResource(selected ? "AppListDetailIconFilled" : "AppListDetailIcon", out var value)
                        ? (IconSource)value
                        : null;
                break;
            }
            case "HotKeyManagerPage":
            {
                item.IconSource = this.TryFindResource(selected ? "KeyboardIconFilled" : "KeyboardIcon", out var value)
                    ? (IconSource)value
                    : null;
                break;
            }
            case "SettingPage":
            {
                item.IconSource = this.TryFindResource(selected ? "SettingsIconFilled" : "SettingsIcon", out var value)
                    ? (IconSource)value
                    : null;
                break;
            }
        }
    }

    private async void AnimateContentForBackButton(bool show)
    {
        if (!WindowIcon.IsVisible)
            return;

        if (show)
        {
            var ani = new Animation
            {
                Duration = TimeSpan.FromMilliseconds(250),
                FillMode = FillMode.Forward,
                Children =
                {
                    new KeyFrame
                    {
                        Cue = new Cue(0d),
                        Setters =
                        {
                            new Setter(MarginProperty, new Thickness(12, 4, 12, 4))
                        }
                    },
                    new KeyFrame
                    {
                        Cue = new Cue(1d),
                        KeySpline = new KeySpline(0, 0, 0, 1),
                        Setters =
                        {
                            new Setter(MarginProperty, new Thickness(48, 4, 12, 4))
                        }
                    }
                }
            };

            await ani.RunAsync(WindowIcon);

            NavView.IsBackButtonVisible = true;
        }
        else
        {
            NavView.IsBackButtonVisible = false;

            var ani = new Animation
            {
                Duration = TimeSpan.FromMilliseconds(250),
                FillMode = FillMode.Forward,
                Children =
                {
                    new KeyFrame
                    {
                        Cue = new Cue(0d),
                        Setters =
                        {
                            new Setter(MarginProperty, new Thickness(48, 4, 12, 4))
                        }
                    },
                    new KeyFrame
                    {
                        Cue = new Cue(1d),
                        KeySpline = new KeySpline(0, 0, 0, 1),
                        Setters =
                        {
                            new Setter(MarginProperty, new Thickness(12, 4, 12, 4))
                        }
                    }
                }
            };

            await ani.RunAsync(WindowIcon);
        }
    }

    private void NavView_OnBackRequested(object? sender, NavigationViewBackRequestedEventArgs e)
    {
        FrameView.GoBack();
    }
}