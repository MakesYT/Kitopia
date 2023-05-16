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
}