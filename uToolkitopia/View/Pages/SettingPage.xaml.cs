using System;
using System.Windows;
using System.Windows.Controls;
using Core.SDKs.Services;
using Core.ViewModel.Pages;
using Microsoft.Extensions.DependencyInjection;

namespace Kitopia.View.Pages;

public partial class SettingPage :  Page
{
  
    public SettingPage()
    {
       
        DataContext = ServiceManager.Services.GetService<SettingPageViewModel>();
        InitializeComponent();
    }

    private void NumberBox_OnValueChanged(object sender, RoutedEventArgs e)
    {
        Console.WriteLine(2);
    }
}