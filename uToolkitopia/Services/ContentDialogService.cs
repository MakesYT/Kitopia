using System;
using System.Windows.Controls;
using Core.SDKs.Services;
using Wpf.Ui.Controls;

namespace Kitopia.Services;

public class ContentDialogService : IContentDialog
{
    public void ShowDialogAsync(object contentPresenter, string title, object content, Action? yes, Action? no)
    {
        var dialog = new ContentDialog((ContentPresenter)contentPresenter)
        {
            Title = title,
            Content = content,
            CloseButtonText = "关闭",
            PrimaryButtonText = "确定",
        };
        dialog.ShowAsync().ContinueWith((e) =>
        {
            switch (e.Result)
            {
                case ContentDialogResult.Primary:
                {
                    yes.Invoke();
                    break;
                }
                case ContentDialogResult.None:
                {
                    no.Invoke();
                    break;
                }
            }
        });
    }

    public void ShowDialog(object contentPresenter, string title, object content, string CloseButtonText,
        string SecondaryButtonText, string PrimaryButtonText, Action? yes, Action? no, Action? cancel)
    {
        var dialog = new ContentDialog((ContentPresenter)contentPresenter)
        {
            Title = title,
            Content = content,
            CloseButtonText = CloseButtonText,
            SecondaryButtonText = SecondaryButtonText,
            PrimaryButtonText = PrimaryButtonText,
        };
        dialog.ShowAsync().ContinueWith((e) =>
        {
            switch (e.Result)
            {
                case ContentDialogResult.Primary:
                {
                    yes.Invoke();
                    break;
                }
                case ContentDialogResult.None:
                {
                    cancel.Invoke();
                    break;
                }
                case ContentDialogResult.Secondary:
                {
                    no.Invoke();
                    break;
                }
            }
        });
    }
}