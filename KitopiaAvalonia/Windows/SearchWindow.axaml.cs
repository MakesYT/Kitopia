using System;
using System.Diagnostics;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Messaging;
using Core.SDKs.Services;
using Core.ViewModel;
using Microsoft.Extensions.DependencyInjection;
using PluginCore;
using Vanara.PInvoke;

namespace KitopiaAvalonia.Windows;

public partial class SearchWindow : Window
{
    public SearchWindow()
    {
        InitializeComponent();
        RenderOptions.SetTextRenderingMode(this, TextRenderingMode.Antialias);
        WeakReferenceMessenger.Default.Register<string, string>(this, "SearchWindowClose", (_, _) =>
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                IsVisible = false;
            });
        });
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

    private void Button_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Down:
                break;
            case Key.Up:
                break;
            case Key.Enter:
                ((Button)sender).Command.Execute(((Button)sender).DataContext);
                break;
            default:
            {
                if (e.Key != Key.System)
                {
                    tx.Focus();
                }

                break;
            }
        }
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
            return;
        }
        else if (e.Key == Key.Down)
        {
            var keyArgs = new KeyEventArgs() { Key = Key.Tab };
            keyArgs.RoutedEvent = KeyDownEvent;
            RaiseEvent(keyArgs);
            e.Handled = true;
            return;
        }

        dataGrid.SelectedIndex = -1;
        Debug.WriteLine("Key Down");
    }

    private void DataGrid_OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            var item = (SearchViewItem?)dataGrid.SelectedItem;
            ((ISearchItemTool)ServiceManager.Services.GetService(typeof(ISearchItemTool))!).OpenFile(item);
        }
    }
}