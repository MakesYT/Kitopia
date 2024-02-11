using System;
using Core.SDKs.Services;
using Core.ViewModel;
using Kitopia.View;
using KitopiaAvalonia.Windows;
using Microsoft.Extensions.DependencyInjection;
using PluginCore;
using Vanara.PInvoke;

namespace Kitopia.Services;

public class SearchItemChooseService : ISearchItemChooseService
{
    public void Choose(Action<SearchViewItem> action)
    {
        ServiceManager.Services.GetService<SearchWindowViewModel>()!.SetSelectMode(true, action);
        ServiceManager.Services.GetService<SearchWindow>()!.Show();

        ServiceManager.Services.GetService<SearchWindow>()!.Focus();
        ServiceManager.Services.GetService<SearchWindow>()!.tx.Focus();
    }
}