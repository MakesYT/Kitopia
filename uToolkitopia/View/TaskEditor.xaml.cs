#region

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using Core.SDKs.Services.Config;
using Core.ViewModel.TaskEditor;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

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

    public void LoadTask(CustomScenario name)
    {
        ((TaskEditorViewModel)DataContext).Load(name);
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);
        // 获取窗体句柄
        var m_Hwnd = new WindowInteropHelper(this).Handle;
        ApplicationThemeManager.Changed += (theme, accent) =>
        {
            WindowBackdrop.ApplyBackdrop(m_Hwnd, WindowBackdropType.Acrylic);
        };
        ((TaskEditorViewModel)DataContext).ContentPresenter = this.ContentPresenter;
    }

    private void ListBox_OnMouseMove(object sender, MouseEventArgs e)
    {
        if (sender is ListBox listBox && e.LeftButton == MouseButtonState.Pressed && listBox.SelectedItem != null)
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

    private void NodifyEditor_Drop(object sender, DragEventArgs e)
    {
        //throw new System.NotImplementedException();
        if (e.Data.GetData(typeof(PointItem)) is PointItem fromListNode)
        {
            if (e.OriginalSource is Border)
            {
                var command = add.Command as ICommand;
                if (command != null &&
                    command.CanExecute(fromListNode)) // Check if the command is not null and can be executed
                {
                    var point = e.GetPosition((IInputElement)Editor);
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
            eventArg.RoutedEvent = UIElement.MouseWheelEvent;
            eventArg.Source = sender;
            var parent = ((Control)sender).Parent as UIElement;
            parent.RaiseEvent(eventArg);
        }
    }
}