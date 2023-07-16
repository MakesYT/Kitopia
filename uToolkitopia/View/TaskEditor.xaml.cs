using System.Windows;

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
}