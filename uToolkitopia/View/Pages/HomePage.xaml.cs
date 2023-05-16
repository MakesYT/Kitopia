using System.Windows.Controls;
using Core.SDKs.Services;
using Core.ViewModel.Pages;
using Microsoft.Extensions.DependencyInjection;

namespace Kitopia.View.Pages;

public partial class HomePage :  Page
{
  
    public HomePage()
    {
       
        DataContext = ServiceManager.Services.GetService<HomePageViewModel>();
        InitializeComponent();
    }
}