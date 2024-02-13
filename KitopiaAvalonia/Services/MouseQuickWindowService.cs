using Avalonia.Threading;
using Core.SDKs.Services;
using KitopiaAvalonia.Windows;
using Microsoft.Extensions.DependencyInjection;

namespace KitopiaAvalonia.Services;

public class MouseQuickWindowService : IMouseQuickWindowService
{
    public void Open()
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            ServiceManager.Services.GetService<MouseQuickWindow>()!.Show();
        });
    }
}