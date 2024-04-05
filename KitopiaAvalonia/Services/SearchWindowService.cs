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
            var searchWindow = ServiceManager.Services.GetService<SearchWindow>();
            if (searchWindow!.IsVisible)
            {
                searchWindow!.IsVisible = false;
            }
            else
            {
                ServiceManager.Services.GetService<SearchWindowViewModel>()!.CheckClipboard();
                ServiceManager.Services.GetService<IWindowTool>()!.MoveWindowToMouseScreenCenter(searchWindow);
                searchWindow.Show();
                ServiceManager.Services.GetService<IWindowTool>()!.SetForegroundWindow(searchWindow!.TryGetPlatformHandle()!.Handle);
                
                searchWindow.Focus();
                searchWindow.tx.Focus();
                searchWindow.tx.SelectAll();
                Task.Run(() =>
                {
                    Thread.CurrentThread.Priority = ThreadPriority.Lowest;
                    ServiceManager.Services.GetService<SearchWindowViewModel>()!.ReloadApps();
                });
            }
        });
    }
}