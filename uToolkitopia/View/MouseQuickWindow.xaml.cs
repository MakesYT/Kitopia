using System;
using System.Windows;

namespace Kitopia.View;

public partial class MouseQuickWindow : Window
{
    public MouseQuickWindow()
    {
        InitializeComponent();
    }

    private void MouseQuickWindow_OnDeactivated(object? sender, EventArgs e)
    {
        this.Close();
    }
}