#region

using System;
using Core.SDKs.Services;
using Microsoft.Extensions.DependencyInjection;
using Wpf.Ui;

#endregion

namespace Kitopia.Services;

public class NavigationPageService : INavigationPageService
{
    public bool Navigate(Type pageType) =>
        ServiceManager.Services!.GetService<INavigationService>()!.Navigate(pageType);

    public bool Navigate(string pageIdOrTargetTag) =>
        ServiceManager.Services!.GetService<INavigationService>()!.Navigate(pageIdOrTargetTag);
}