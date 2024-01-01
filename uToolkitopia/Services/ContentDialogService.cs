#region

using System;
using System.Windows.Controls;
using Core.SDKs;
using Core.SDKs.Services;

#endregion

namespace Kitopia.Services;

public class ContentDialogService : IContentDialog
{
    public void ShowDialog(object contentPresenter, DialogContent dialogContent)
    {
        var dialog = new ContentDialog((ContentPresenter)contentPresenter)
        {
            Title = dialogContent.Title,
            Content = dialogContent.Content,
            CloseButtonText = dialogContent.CloseButtonText,
            SecondaryButtonText = dialogContent.SecondaryButtonText,
            PrimaryButtonText = dialogContent.PrimaryButtonText
        };
        dialog.ShowAsync().ContinueWith(e =>
        {
            switch (e.Result)
            {
                case ContentDialogResult.Primary:
                {
                    dialogContent.Yes.Invoke();
                    break;
                }
                case ContentDialogResult.None:
                {
                    dialogContent.Cancel.Invoke();
                    break;
                }
                case ContentDialogResult.Secondary:
                {
                    dialogContent.No.Invoke();
                    break;
                }
            }
        });
    }

    public void ShowDialogAsync(object contentPresenter, string title, object content, Action? yes, Action? no)
    {
        var dialog = new ContentDialog((ContentPresenter)contentPresenter)
        {
            Title = title,
            Content = content,
            CloseButtonText = "关闭",
            PrimaryButtonText = "确定"
        };
        dialog.ShowAsync().ContinueWith(e =>
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
}