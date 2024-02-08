#region

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using Core.SDKs.CustomScenario;
using Core.SDKs.Services;
using Core.ViewModel;
using Core.ViewModel.TaskEditor;
using FluentAvalonia.UI.Windowing;
using KitopiaAvalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using PluginCore;
using Vanara.PInvoke;
using DataObject = Avalonia.Input.DataObject;
using DragDrop = Avalonia.Input.DragDrop;
using DragDropEffects = Avalonia.Input.DragDropEffects;
using DragEventArgs = Avalonia.Input.DragEventArgs;
using Point = Avalonia.Point;
using RoutedEventArgs = Avalonia.Interactivity.RoutedEventArgs;

#endregion

namespace KitopiaAvalonia.Windows;

public partial class TaskEditor : AppWindow
{
    public TaskEditor()
    {
        InitializeComponent();
        RenderOptions.SetTextRenderingMode(this, TextRenderingMode.Antialias);
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var size = desktop.MainWindow.Screens.Primary.Bounds.Size;
            Width = size.Width * 2 / 3;
            Height = size.Height * 2 / 3;
        }

        Editor.AddHandler(DragDrop.DropEvent, NodifyEditor_Drop);
    }

    public void LoadTask(CustomScenario name) => ((TaskEditorViewModel)DataContext).Load(name);

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        //((TaskEditorViewModel)DataContext).ContentPresenter = ContentPresenter;
    }


    private void ListBox_OnMouseMove(object? sender, PointerEventArgs e)
    {
        var point = e.GetCurrentPoint(this);
        if (sender is ListBox listBox)
        {
            if (point.Properties.IsLeftButtonPressed && listBox.SelectedItem != null)
            {
                try
                {
                    var data = new DataObject();
                    data.Set("KitopiaPointItem", listBox.SelectedItem);

                    DragDrop.DoDragDrop(e, data, DragDropEffects.Copy);
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception);
                }
            }
        }
        else if (sender is Border border)
        {
            if (point.Properties.IsLeftButtonPressed)
            {
                var borderDataContext = border.DataContext;
                try
                {
                    var keyValuePair = (KeyValuePair<string, object>)borderDataContext;
                    var pointItem = new PointItem()
                    {
                        Title = $"变量:{keyValuePair.Key}",
                        MerthodName = "valueSet",
                        ValueRef = keyValuePair.Key,
                        Plugin = "Kitopia"
                    };
                    if ((string)border.Tag == "Set")
                    {
                        ObservableCollection<ConnectorItem> inpItems = new();
                        inpItems.Add(new ConnectorItem()
                        {
                            Source = pointItem,
                            Type = typeof(NodeConnectorClass),
                            Title = "流输入",
                            TypeName = "节点"
                        });
                        inpItems.Add(new ConnectorItem()
                        {
                            Source = pointItem,
                            Type = typeof(object),
                            Title = "设置",
                            TypeName = "变量"
                        });
                        pointItem.Input = inpItems;
                    }
                    else if ((string)border.Tag == "Get")
                    {
                        ObservableCollection<ConnectorItem> inpItems = new();
                        inpItems.Add(new ConnectorItem()
                        {
                            Source = pointItem,
                            Type = typeof(object),
                            Title = "获取",
                            IsOut = true,
                            TypeName = "变量"
                        });
                        pointItem.MerthodName = "valueGet";
                        pointItem.Output = inpItems;
                    }

                    var data = new DataObject();
                    data.Set("KitopiaPointItem", pointItem);
                    DragDrop.DoDragDrop(e, data, DragDropEffects.Copy);
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception);
                }
            }
        }
    }

    private void NodifyEditor_Drop(object sender, DragEventArgs e)
    {
        //throw new System.NotImplementedException();
        if (e.Data.Get("KitopiaPointItem") is PointItem fromListNode)
        {
            var command = add.Command;
            if (command != null &&
                command.CanExecute(fromListNode)) // Check if the command is not null and can be executed
            {
                var point = e.GetPosition(Editor);
                point -= new Point(Editor.ViewTranslateTransform.X, Editor.ViewTranslateTransform.Y);
                fromListNode.Location = point;
                command.Execute(fromListNode); // Pass null or any other parameter as needed
            }
        }
    }

    private void NodifyEditor_DragOver(object sender, DragEventArgs e)
    {
        //throw new System.NotImplementedException();
    }

    private void NodifyEditor_DragEnter(object sender, DragEventArgs e)
    {
        //throw new System.NotImplementedException();
    }

    private void NodifyEditor_DragLeave(object sender, DragEventArgs e)
    {
        //throw new System.NotImplementedException();
    }


    private void SearchItemShow_OnClick(object? sender, RoutedEventArgs routedEventArgs)
    {
        ServiceManager.Services.GetService<SearchWindowViewModel>()!.SetSelectMode(true, (item =>
        {
            Dispatcher.UIThread.Post(() =>
            {
                ((SearchItemShow)sender).OnlyKey = item.OnlyKey;
            });
        }));
        ServiceManager.Services.GetService<SearchWindow>()!.Show();

        User32.SetForegroundWindow(
            ServiceManager.Services.GetService<SearchWindow>()!.TryGetPlatformHandle().Handle);
        ServiceManager.Services.GetService<SearchWindow>()!.tx.Focus();
    }
}