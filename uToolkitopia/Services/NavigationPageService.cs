using System;
using Core.SDKs.Services;
using Microsoft.Extensions.DependencyInjection;
using Wpf.Ui.Contracts;

namespace Kitopia.Services;

public class NavigationPageService : INavigationPageService
{
    public bool Navigate(Type pageType)
    {
        return ServiceManager.Services!.GetService<INavigationService>()!.Navigate(pageType);
    }

    public bool Navigate(string pageIdOrTargetTag)
    {
        return ServiceManager.Services!.GetService<INavigationService>()!.Navigate(pageIdOrTargetTag);
    }
}