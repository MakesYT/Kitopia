#region

using log4net;
using Microsoft.Toolkit.Uwp.Notifications;
using PluginCore;

#endregion

namespace KitopiaAvalonia.Services;

public class ToastService : IToastService
{
    private static readonly ILog log = LogManager.GetLogger(nameof(ToastService));


    public void Show(string header, string text)
    {
        log.Debug(nameof(ToastService) + "的接口" + nameof(Show) + "被调用");
        new ToastContentBuilder()
           .AddText(header)
           .AddText(text)
           .Show();
    }
}