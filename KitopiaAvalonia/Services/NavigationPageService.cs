using System;
using Core.SDKs.Services;
using Microsoft.Extensions.DependencyInjection;

namespace KitopiaAvalonia.Services;

public class NavigationPageService : INavigationPageService
{
    public bool Navigate(Type pageType)
    {
        throw new NotImplementedException();
    }

    public bool Navigate(string pageIdOrTargetTag)
    {
        ServiceManager.Services.GetService<MainWindow>().FrameView.NavigateFromObject(pageIdOrTargetTag);
        return true;
    }
}