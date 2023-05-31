﻿using System;
using System.Windows;
using Core.SDKs.Config;
using Core.SDKs.Services;
using log4net;
using Microsoft.Toolkit.Uwp.Notifications;
using MessageBox = Wpf.Ui.Controls.MessageBoxControl.MessageBox;
using MessageBoxResult = Wpf.Ui.Controls.MessageBoxControl.MessageBoxResult;

namespace Kitopia.Services;

public class ToastService : IToastService
{
    private static readonly ILog log = LogManager.GetLogger(nameof(ToastService));

    public void show(string text)
    {
        if (ConfigManger.config.debugMode)
        {
            log.Debug(nameof(ToastService) + "的接口" + nameof(show) + "被调用");
        }

        new ToastContentBuilder()
            .AddText(text)
            .Show();
    }

    public void showMessageBox(string Title, string Content, Action yesAction, Action noAction)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            MessageBox msg = new MessageBox();
            msg.Title = Title;
            msg.Content = Content;
            msg.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            msg.CloseButtonText = "取消";
            msg.PrimaryButtonText = "确定";
            msg.FontSize = 15;
            var task = msg.ShowDialogAsync();
            // 使用ContinueWith来在任务完成后执行一个回调函数
            task.ContinueWith(e =>
            {
                MessageBoxResult result = e.Result;
                switch (result)
                {
                    case MessageBoxResult.Primary:
                    {
                        yesAction.Invoke();


                        break;
                    }
                    case MessageBoxResult.None:
                    {
                        noAction.Invoke();
                        break;
                    }
                }
            });
        });
    }
}