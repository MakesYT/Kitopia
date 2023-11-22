using System;
using System.Windows.Interop;
using Core.SDKs.Services;
using Core.ViewModel;
using Kitopia.View;
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

        User32.SetForegroundWindow(
            new WindowInteropHelper(ServiceManager.Services.GetService<SearchWindow>()!)
                .Handle);
        ServiceManager.Services.GetService<SearchWindow>()!.tx.Focus();
    }
}