using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using Core.SDKs.Services;
using Core.ViewModel;
using KitopiaAvalonia.Windows;
using Microsoft.Extensions.DependencyInjection;

namespace KitopiaAvalonia.Services;

public class SearchWindowService : ISearchWindowService
{
    public void ShowOrHiddenSearchWindow()
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            if (ServiceManager.Services.GetService<SearchWindow>()!.IsVisible)
            {
                ServiceManager.Services.GetService<SearchWindow>()!.IsVisible = false;
            }
            else
            {
                ServiceManager.Services.GetService<SearchWindowViewModel>()!.CheckClipboard();

                ServiceManager.Services.GetService<SearchWindow>()!.Show();

                ServiceManager.Services.GetService<SearchWindow>()!.Focus();
                ServiceManager.Services.GetService<SearchWindow>()!.tx.Focus();
                ServiceManager.Services.GetService<SearchWindow>()!.tx.SelectAll();
                Task.Run(() =>
                {
                    Thread.CurrentThread.Priority = ThreadPriority.Lowest;
                    ServiceManager.Services.GetService<SearchWindowViewModel>()!.ReloadApps();
                });
            }
        });
    }
}