#region

using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Threading;
using log4net;
using PluginCore;

#endregion

namespace KitopiaAvalonia.Services;

public class ToastService : IToastService
{
    private static readonly ILog log = LogManager.GetLogger(nameof(ToastService));
    private INotificationManager notificationManager;

    public void Init(TopLevel mainWindow)
    {
        if (notificationManager is not null)
        {
            throw new InvalidOperationException("已初始化");
        }

        log.Debug(nameof(ToastService) + "的接口" + nameof(Init) + "被调用");

        notificationManager = new WindowNotificationManager(mainWindow);
    }


    public void Show(string header, string text)
    {
        if (notificationManager is null)
        {
            throw new InvalidOperationException("未初始化");
        }

        log.Debug(nameof(ToastService) + "的接口" + nameof(Show) + "被调用");

        Dispatcher.UIThread.InvokeAsync(() =>
        {
            notificationManager.Show(new Notification(header, text));
        });

    }
}