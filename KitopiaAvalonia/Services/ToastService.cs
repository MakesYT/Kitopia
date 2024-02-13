#region

using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Threading;
using log4net;
using PluginCore;
using Notification = DesktopNotifications.Notification;

#endregion

namespace KitopiaAvalonia.Services;

public class ToastService : IToastService
{
    private static readonly ILog log = LogManager.GetLogger(nameof(ToastService));


    public void Show(string header, string text)
    {
        log.Debug(nameof(ToastService) + "的接口" + nameof(Show) + "被调用");

        Dispatcher.UIThread.InvokeAsync(() =>
        {
            Program.NotificationManager.ShowNotification(new Notification() { Title = header, Body = text });
        });
    }
}