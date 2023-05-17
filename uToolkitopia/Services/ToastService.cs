using Core.SDKs.Services;
using Microsoft.Toolkit.Uwp.Notifications;

namespace Kitopia.Services;

public class ToastService :IToastService
{
    public void show(string text)
    {
        new ToastContentBuilder()
            .AddText(text)
            .Show();   
    }
}