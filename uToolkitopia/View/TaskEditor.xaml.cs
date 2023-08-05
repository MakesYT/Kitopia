using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Core.ViewModel.TaskEditor;

namespace Kitopia.View;

public partial class TaskEditor
{
    public TaskEditor()
    {
        InitializeComponent();

        this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        this.Width = SystemParameters.PrimaryScreenWidth * 2 / 3;
        this.Height = SystemParameters.PrimaryScreenHeight * 2 / 3;
    }

    private void ListBox_OnMouseMove(object sender, MouseEventArgs e)
    {
        if (sender is ListBox listBox && e.LeftButton == MouseButtonState.Pressed && listBox.SelectedItem != null)
        {
            DragDrop.DoDragDrop(listBox, listBox.SelectedItem, DragDropEffects.Copy);
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
                    Point point = (e.GetPosition((IInputElement)Editor));
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
}