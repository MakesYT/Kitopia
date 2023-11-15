using System.ComponentModel;
using System.Windows;
using Core.SDKs.Services;
using Core.ViewModel;
using Microsoft.Extensions.DependencyInjection;
using PluginCore;
using Wpf.Ui.Controls;

namespace Kitopia.Controls;

public class SearchItemShow : Button
{
    public static readonly DependencyProperty SearchViewItemProperty = DependencyProperty.Register(
        nameof(SearchViewItem),
        typeof(SearchViewItem), typeof(SearchItemShow),
        new PropertyMetadata(null));

    public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register(nameof(IsSelected),
        typeof(bool), typeof(SearchItemShow),
        new PropertyMetadata(false));


    public static readonly DependencyProperty OnlyKeyProperty = DependencyProperty.Register(nameof(OnlyKey),
        typeof(string), typeof(SearchItemShow),
        new PropertyMetadata("", OnOnlyKeyChanged));

    [Bindable(true)]
    [Category("SearchViewItem")]
    public SearchViewItem SearchViewItem
    {
        get => (SearchViewItem)GetValue(SearchViewItemProperty);
        set => SetValue(SearchViewItemProperty, value);
    }

    [Bindable(true)]
    [Category("IsSelected")]
    public bool IsSelected
    {
        get => (bool)GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }

    [Bindable(true)]
    [Category("OnlyKey")]
    public string OnlyKey
    {
        get => (string)GetValue(OnlyKeyProperty);
        set => SetValue(OnlyKeyProperty, value);
    }

    private static void OnOnlyKeyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (SearchItemShow)d;
        var value = (string)e.NewValue;
        if (value is null)
        {
            return;
        }

        if (ServiceManager.Services.GetService<SearchWindowViewModel>()!._collection.TryGetValue(value,
                out var searchViewItem))
        {
            control.SearchViewItem = searchViewItem;
            control.IsSelected = true;
        }
        else
        {
            control.SearchViewItem = null;
            control.IsSelected = false;
        }
    }
}