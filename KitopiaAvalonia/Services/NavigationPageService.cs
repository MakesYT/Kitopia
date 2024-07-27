using System;
using CommunityToolkit.Mvvm.Messaging;
using Core.SDKs.Services;
using Core.ViewModel;

namespace KitopiaAvalonia.Services;

public class NavigationPageService : INavigationPageService
{
    public bool Navigate(Type pageType)
    {
        throw new NotImplementedException();
    }

    public bool Navigate(string pageIdOrTargetTag)
    {
        WeakReferenceMessenger.Default.Send<PageChangeEventArgs>(new PageChangeEventArgs(pageIdOrTargetTag));

        return true;
    }
}