#region

using System;
using System.Windows;
using Core.SDKs.Services;
using log4net;
using Microsoft.Toolkit.Uwp.Notifications;
using MessageBox = Kitopia.Controls.MessageBoxControl.MessageBox;
using MessageBoxResult = Kitopia.Controls.MessageBoxControl.MessageBoxResult;

#endregion

namespace Kitopia.Services;

public class ToastService : IToastService
{
    private static readonly ILog log = LogManager.GetLogger(nameof(ToastService));

    public void show(string text)
    {
        log.Debug(nameof(ToastService) + "的接口" + nameof(show) + "被调用");


        new ToastContentBuilder()
            .AddText(text)
            .Show();
    }

    public void showMessageBox(string Title, string Content, Action? yesAction, Action? noAction) =>
        Application.Current.Dispatcher.BeginInvoke(() =>
        {
            var msg = new MessageBox();
            msg.Title = Title;
            msg.Content = Content;
            msg.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            if (noAction is not null)
            {
                msg.CloseButtonText = "取消";
            }

            msg.PrimaryButtonText = "确定";
            msg.FontSize = 15;
            var task = msg.ShowDialogAsync();
            // 使用ContinueWith来在任务完成后执行一个回调函数
            task.ContinueWith(e =>
            {
                var result = e.Result;
                switch (result)
                {
                    case MessageBoxResult.Primary:
                    {
                        yesAction?.Invoke();
                        break;
                    }
                    case MessageBoxResult.None:
                    {
                        noAction?.Invoke();
                        break;
                    }
                }
            });
        });

    public void showMessageBoxW(string title, object content, string CloseButtonText, string SecondaryButtonText,
        string PrimaryButtonText, Action? yes, Action? no, Action? cancel) =>
        Application.Current.Dispatcher.BeginInvoke(() =>
        {
            var msg = new MessageBox();
            msg.Title = title;
            msg.Content = content;
            msg.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            if (no is not null)
            {
                msg.SecondaryButtonText = SecondaryButtonText;
            }

            if (cancel is not null)
            {
                msg.CloseButtonText = CloseButtonText;
            }

            msg.PrimaryButtonText = PrimaryButtonText;
            msg.FontSize = 15;
            var task = msg.ShowDialogAsync().GetAwaiter();

            // 使用ContinueWith来在任务完成后执行一个回调函数


            switch (task.GetResult())
            {
                case MessageBoxResult.Primary:
                {
                    yes?.Invoke();
                    break;
                }
                case MessageBoxResult.None:
                {
                    cancel?.Invoke();
                    break;
                }
                case MessageBoxResult.Secondary:
                {
                    no?.Invoke();
                    break;
                }
            }
        });
}