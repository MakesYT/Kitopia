using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Messaging;
using Core.SDKs.Services;
using Core.ViewModel;
using KitopiaAvalonia.Tools;
using Microsoft.Extensions.DependencyInjection;
using PluginCore;

namespace KitopiaAvalonia.Windows;

public partial class SearchWindow : Window
{
    public SearchWindow()
    {
        InitializeComponent();
        RenderOptions.SetTextRenderingMode(this, TextRenderingMode.Antialias);
        WeakReferenceMessenger.Default.Register<string, string>(this, "SearchWindowClose",
            (_, _) => { Dispatcher.UIThread.InvokeAsync(() => { IsVisible = false; }); });
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var size = desktop.MainWindow.Screens.Primary.Bounds.Size;
            Position = new PixelPoint((int)((size.Width - Width) / 2), size.Height / 4);
        }
    }

    private void w_Deactivated(object? sender, EventArgs eventArgs)
    {
        IsVisible = false;
    }


    private void w_Activated(object sender, EventArgs e)
    {
        this.Focus();
    }

    private void tx_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            if (dataGrid.Items.Count != 0)
            {
                ServiceManager.Services!.GetService<SearchWindowViewModel>()!.OpenFile(
                    (SearchViewItem)dataGrid.Items[0]);
            }

            e.Handled = true;
        }
        else if (e.Key == Key.Down)
        {
            var realizedContainers = dataGrid.GetRealizedContainers();
            dataGrid.SelectedItem = (object)((SearchWindowViewModel)DataContext).Items[0];
            foreach (var realizedContainer in realizedContainers)
            {
                if (realizedContainer.DataContext == dataGrid.SelectedItem)
                {
                    realizedContainer.Focus();
                    break;
                }
            }
        }
    }

    private void DataGrid_OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Up)
        {
            var realizedContainers = dataGrid.GetRealizedContainers();
            if (realizedContainers.First()
                                  .DataContext == dataGrid.SelectedItem)
            {
                tx.Focus();
            }

            return;
        }

        if (e.Key == Key.Enter)
        {
            var item = (SearchViewItem?)dataGrid.SelectedItem;
            ((ISearchItemTool)ServiceManager.Services.GetService(typeof(ISearchItemTool))!).OpenFile(item);
            return;
        }

        if (e.Key != Key.Down && e.Key != Key.Home && e.Key != Key.End &&
            e.Key != Key.Left && e.Key != Key.Right && e.Key != Key.Tab && e.Key != Key.PageDown && e.Key != Key.PageUp)
        {
            tx.Focus();
        }
    }

    private void DataGrid_OnPointerMoved(object? sender, PointerEventArgs e)
    {
        var listBoxItem = dataGrid.GetVisualAt<ListBoxItem>(e.GetCurrentPoint(dataGrid)
                                                             .Position);
        if (listBoxItem != null)
        {
            listBoxItem.IsSelected = true;
        }
    }

    private void InputElement_OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            IsVisible = false;
        }
    }
}