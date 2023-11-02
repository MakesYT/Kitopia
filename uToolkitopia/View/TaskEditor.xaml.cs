#region

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using Core.SDKs.CustomScenario;
using Core.SDKs.Services;
using Core.ViewModel;
using Core.ViewModel.TaskEditor;
using Kitopia.Controls;
using Microsoft.Extensions.DependencyInjection;
using PluginCore;
using Vanara.PInvoke;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;
using MouseButtonState = System.Windows.Input.MouseButtonState;

#endregion

namespace Kitopia.View;

public partial class TaskEditor
{
    public TaskEditor()
    {
        InitializeComponent();

        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        Width = SystemParameters.PrimaryScreenWidth * 2 / 3;
        Height = SystemParameters.PrimaryScreenHeight * 2 / 3;
    }

    public void LoadTask(CustomScenario name) => ((TaskEditorViewModel)DataContext).Load(name);

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);
        // 获取窗体句柄
        var m_Hwnd = new WindowInteropHelper(this).Handle;
        ApplicationThemeManager.Changed += (theme, accent) =>
        {
            WindowBackdrop.ApplyBackdrop(m_Hwnd, WindowBackdropType.Acrylic);
        };
        ((TaskEditorViewModel)DataContext).ContentPresenter = ContentPresenter;
    }

    private void ListBox_OnMouseMove(object sender, MouseEventArgs e)
    {
        if (sender is ListBox listBox)
        {
            if (e.LeftButton == MouseButtonState.Pressed && listBox.SelectedItem != null)
            {
                try
                {
                    DragDrop.DoDragDrop(listBox, listBox.SelectedItem, DragDropEffects.Copy);
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception);
                }
            }
        }
        else if (sender is Border border)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                try
                {
                    var keyValuePair = (KeyValuePair<string, object>)border.DataContext;
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


                    DragDrop.DoDragDrop(border, pointItem, DragDropEffects.Copy);
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
        if (e.Data.GetData(typeof(PointItem)) is PointItem fromListNode)
        {
            if (e.OriginalSource is Border)
            {
                var command = add.Command;
                if (command != null &&
                    command.CanExecute(fromListNode)) // Check if the command is not null and can be executed
                {
                    var point = e.GetPosition(Editor);
                    point.X += Editor.ViewportLocation.X;
                    point.Y += Editor.ViewportLocation.Y;
                    fromListNode.Location = point;
                    command.Execute(fromListNode); // Pass null or any other parameter as needed
                }
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

    private void ListBox_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (!e.Handled)
        {
            // ListView拦截鼠标滚轮事件
            e.Handled = true;

            // 激发一个鼠标滚轮事件，冒泡给外层ListView接收到
            var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta);
            eventArg.RoutedEvent = MouseWheelEvent;
            eventArg.Source = sender;
            var parent = ((Control)sender).Parent as UIElement;
            parent.RaiseEvent(eventArg);
        }
    }

    private void SearchItemShow_OnClick(object sender, RoutedEventArgs e)
    {
        ServiceManager.Services.GetService<SearchWindowViewModel>()!.SetSelectMode(true, (item =>
        {
            Application.Current.Dispatcher.BeginInvoke(() =>
            {
                ((SearchItemShow)sender).OnlyKey = item.OnlyKey;
            });
        }));
        ServiceManager.Services.GetService<SearchWindow>()!.Show();

        User32.SetForegroundWindow(
            new WindowInteropHelper(ServiceManager.Services.GetService<SearchWindow>()!)
                .Handle);
        ServiceManager.Services.GetService<SearchWindow>()!.tx.Focus();
    }
}