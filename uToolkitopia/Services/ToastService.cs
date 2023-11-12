#region

using System;
using System.Windows;
using log4net;
using Microsoft.Toolkit.Uwp.Notifications;
using PluginCore;
using MessageBox = Kitopia.Controls.MessageBoxControl.MessageBox;
using MessageBoxResult = Kitopia.Controls.MessageBoxControl.MessageBoxResult;

#endregion

namespace Kitopia.Services;

public class ToastService : IToastService
{
    private static readonly ILog log = LogManager.GetLogger(nameof(ToastService));

    public void Show(string header, string text)
    {
        log.Debug(nameof(ToastService) + "的接口" + nameof(Show) + "被调用");


        new ToastContentBuilder()
            .AddHeader("Kitopia", header, "")
            .AddText(text)
            .Show();
    }

    public void ShowMessageBox(string title, string content, Action? yesAction, Action? noAction) =>
        Application.Current.Dispatcher.BeginInvoke(() =>
        {
            var msg = new MessageBox();
            msg.Title = title;
            msg.Content = content;
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

    public void ShowMessageBoxW(string title, object? content, ShowMessageContent showMessageContent) =>
        Application.Current.Dispatcher.BeginInvoke(() =>
        {
            var msg = new MessageBox();
            msg.Title = title;
            msg.Content = content;
            msg.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            if (showMessageContent.SecondaryButtonText is not null)
            {
                msg.SecondaryButtonText = showMessageContent.SecondaryButtonText;
            }

            if (showMessageContent.CloseButtonText is not null)
            {
                msg.CloseButtonText = showMessageContent.CloseButtonText;
            }

            msg.PrimaryButtonText = showMessageContent.PrimaryButtonText;
            msg.FontSize = 15;
            var task = msg.ShowDialogAsync().GetAwaiter();

            // 使用ContinueWith来在任务完成后执行一个回调函数


            switch (task.GetResult())
            {
                case MessageBoxResult.Primary:
                {
                    showMessageContent.Yes?.Invoke();
                    break;
                }
                case MessageBoxResult.None:
                {
                    showMessageContent.Cancel?.Invoke();
                    break;
                }
                case MessageBoxResult.Secondary:
                {
                    showMessageContent.No?.Invoke();
                    break;
                }
            }
        });
}