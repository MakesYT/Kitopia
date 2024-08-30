#region

using System.Threading.Tasks;
using System.Windows.Threading;
using log4net;
using Microsoft.Windows.AppNotifications;
using Microsoft.Windows.AppNotifications.Builder;
using PluginCore;

#endregion

namespace KitopiaAvalonia.Services;

public class ToastService : IToastService
{
    private static readonly ILog log = LogManager.GetLogger(nameof(ToastService));

    public void Init()
    {
        AppNotificationManager notificationManager = AppNotificationManager.Default;

        notificationManager.NotificationInvoked += OnNotificationInvoked;

        notificationManager.Register();
    }

    private void OnNotificationInvoked(AppNotificationManager sender, AppNotificationActivatedEventArgs args)
    {
        
    }


    public void Show(string header, string text)
    {
        log.Debug($"{nameof(ToastService)}的接口{nameof(Show)}被调用,header：{header},text：{text}");
        var appNotification = new AppNotificationBuilder()
            .AddText(header)
            .AddText(text)
            .BuildNotification();
        AppNotificationManager.Default.Show(appNotification);
    }

    public void Unregister()
    {
        AppNotificationManager.Default.Unregister();
    }
}